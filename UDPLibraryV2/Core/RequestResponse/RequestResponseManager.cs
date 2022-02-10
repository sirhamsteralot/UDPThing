using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.PacketQueueing;
using UDPLibraryV2.Core.Packets;
using System.Net;
using UDPLibraryV2.Core.Serialization;

namespace UDPLibraryV2.Core.RequestResponse
{
    public class RequestResponseManager
    {
        UDPCore _udpCore;

        private ConcurrentDictionary<short, Func<IRequest, IResponse>> _procedureCalls;
        private ConcurrentDictionary<short, TaskCompletionSource<ReconstructedPacket>> _receivedResponsePackets;

        public RequestResponseManager(UDPCore core)
        {
            _udpCore = core;

            _udpCore.OnPayloadReceivedEvent += _udpCore_OnPayloadReceivedEvent;

            _procedureCalls = new ConcurrentDictionary<short, Func<IRequest, IResponse>>();
            _receivedResponsePackets = new ConcurrentDictionary<short, TaskCompletionSource<ReconstructedPacket>>();
        }

        private void _udpCore_OnPayloadReceivedEvent(ReconstructedPacket packet, IPEndPoint? source)
        {
            if (_receivedResponsePackets.TryGetValue(packet.StreamId, out TaskCompletionSource<ReconstructedPacket> responsePacket)) {
                responsePacket.TrySetResult(packet);
                return;
            }

            if (_procedureCalls.TryGetValue(packet.TypeId, out Func<IRequest, IResponse> responseFunc))
            {
                Respond(source, responseFunc, packet);
            }
        }

        public void RegisterResponse(short callId, Func<IRequest, IResponse> responseProcedure)
        {
            _procedureCalls[callId] = responseProcedure;
        }

        public async Task<TResponse> Request<TResponse>(IRequest request, IPEndPoint remoteEndPoint) where TResponse : INetworkSerializable, new()
        {
            var streamId = _udpCore.OpenStream(remoteEndPoint);

            byte[] buffer = new byte[request.MinimumBufferSize];

            request.Serialize(buffer, 0);

            PacketFragment fragment = new PacketFragment(buffer, request.TypeId, false);

            var receivedTask = new TaskCompletionSource<ReconstructedPacket>();
            _receivedResponsePackets.TryAdd(streamId, receivedTask);

            _udpCore.QueueFragment(streamId, fragment, SendPriority.Medium);

            ReconstructedPacket responsePacket = receivedTask.Task.GetAwaiter().GetResult();
            _receivedResponsePackets.Remove(streamId, out TaskCompletionSource<ReconstructedPacket> _);

            TResponse response = new TResponse();
            response.Deserialize(responsePacket.GetPayloadBytes(), 0);

            _udpCore.CloseStream(streamId);

            return response;
        }

        private void Respond(IPEndPoint? source, Func<IRequest, IResponse> responseFunc, ReconstructedPacket packet)
        {
            IRequest request = (IRequest)Activator.CreateInstance(TypeProvider.Instance.GetType(packet.TypeId));
            request.Deserialize(packet.GetPayloadBytes(), 0);

            IResponse response = responseFunc(request);
            byte[] buffer = new byte[response.MinimumBufferSize];

            PacketFragment fragment = new PacketFragment(buffer, response.TypeId, false);

            _udpCore.OpenStream(source, packet.StreamId);
            _udpCore.QueueFragment(packet.StreamId, fragment, SendPriority.Medium);
            _udpCore.CloseStream(packet.StreamId);
        }
    }
}
