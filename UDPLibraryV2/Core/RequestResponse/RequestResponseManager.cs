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
    internal class RequestResponseManager
    {
        UDPCore _udpCore;

        private ConcurrentDictionary<short, Func<IRequest, IResponse>> _procedureCalls;
        private ConcurrentDictionary<short, TaskCompletionSource<CompletePacket>> _receivedResponsePackets;

        public RequestResponseManager(UDPCore core)
        {
            _udpCore = core;

            _udpCore.OnPayloadReceivedEvent += _udpCore_OnPayloadReceivedEvent;
        }

        private void _udpCore_OnPayloadReceivedEvent(CompletePacket packet, IPEndPoint? source)
        {
            if (_receivedResponsePackets.TryGetValue(packet.StreamId, out TaskCompletionSource<CompletePacket> responsePacket)) {
                responsePacket.SetResult(packet);
                return;
            }

            if (_procedureCalls.TryGetValue(packet.TypeId, out Func<IRequest, IResponse> responseProc))
            {
                Respond(source, responseProc, packet);
            }
        }

        public async Task<TResponse> Request<TResponse>(IRequest request) where TResponse : INetworkSerializable, new()
        {
            var streamId = _udpCore.GetStreamCodeLock();
            byte[] buffer = new byte[request.MinimumBufferSize];

            request.Serialize(buffer, 0);

            PacketFragment fragment = new PacketFragment(buffer, request.TypeId, false);

            var receivedTask = new TaskCompletionSource<CompletePacket>();
            _receivedResponsePackets.TryAdd(streamId, receivedTask);

            _udpCore.QueueFragment(streamId, fragment, SendPriority.Medium);

            CompletePacket responsePacket = await receivedTask.Task;
            _receivedResponsePackets.Remove(streamId, out TaskCompletionSource<CompletePacket> _);

            TResponse response = new TResponse();
            response.Deserialize(responsePacket.GetPayloadBytes(), 0);

            _udpCore.UnlockStreamCode(streamId);

            return response;
        }

        public void Respond(IPEndPoint? source, Func<IRequest, IResponse> responseProc, CompletePacket packet)
        {
            IRequest request = (IRequest)Activator.CreateInstance(TypeProvider.Instance.GetType(packet.TypeId));
            request.Deserialize(packet.GetPayloadBytes(), 0);

            IResponse response = responseProc(request);
            byte[] buffer = new byte[response.MinimumBufferSize];

            PacketFragment fragment = new PacketFragment(buffer, response.TypeId, false);

            _udpCore.QueueFragment(packet.StreamId, fragment, SendPriority.Medium);
        }
    }
}
