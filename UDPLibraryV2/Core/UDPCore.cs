using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.PacketQueueing;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.Core.Serialization;

namespace UDPLibraryV2.Core
{
    public class UDPCore
    {
        public event Action<CompletePacket, IPEndPoint?> OnPayloadReceivedEvent;

        private bool _receive;
        private int _listenPort;
        private UdpClient _listener;

        private DeconstructionService _deconstructionService;
        private ReconstructionService _reconstructionService;

        private PacketSender _packetSender;

        private List<INetworkService> _services;

        private HashSet<short> _openStreams;

        public UDPCore(IPEndPoint listenIp)
        {
            _listener = new UdpClient(listenIp);
            _listenPort = ((IPEndPoint)_listener.Client.LocalEndPoint).Port;

            _services = new List<INetworkService>();

            _openStreams = new HashSet<short>();

            _deconstructionService = new DeconstructionService(NetworkPacket.payloadMaxSize / 2);

            _reconstructionService = new ReconstructionService(this);
            _reconstructionService.OnPayloadReconstructed += ReconstructionService_OnPayloadReconstructed;
            RegisterService(_reconstructionService);

            _packetSender = new PacketSender(this, 128, 120);
        }

        public void StartListening()
        {
            _receive = true;
            _listener.BeginReceive(NetworkReceiveCallback, null);
        }

        public void StartSending()
        {
            _packetSender.SendNetworkMessages().ContinueWith(x => Console.WriteLine(x.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        public short OpenStream(IPEndPoint endPoint)
        {
            return _packetSender.OpenStream(endPoint);
        }

        public void QueueSerializable(INetworkSerializable serializable, short streamId, bool compression, SendPriority priority)
        {
            byte[] data = new byte[serializable.MinimumBufferSize];
            serializable.Serialize(data, 0);

            var fragments = _deconstructionService.CreateFragments(data, serializable.TypeId, compression);
            foreach (var fragment in fragments)
            {
                QueueFragment(streamId, fragment, priority);
            }
        }

        internal void RegisterService(INetworkService service)
        {
            _services.Add(service);
        }

        internal short GetStreamCodeLock()
        {
            short value = 0;
            do
            {
                value = (short)Random.Shared.NextInt64();

            }
            while (_openStreams.Contains(value));

            return value;
        }

        internal void UnlockStreamCode(short code)
        {
            _openStreams.Remove(code);
        }

        internal async Task SendBytesAsync(byte[] bytes, int amountToSend, IPEndPoint endpoint)
        {
            await _listener.SendAsync(bytes, amountToSend, endpoint);
        }

        internal void QueueFragment(short streamid, PacketFragment fragment, SendPriority priority)
        {
            _packetSender.QueueFragment(streamid, fragment, priority);
        }

        private void ReconstructionService_OnPayloadReconstructed(CompletePacket packet, IPEndPoint? sourceEP)
        {
            OnPayloadReceivedEvent?.Invoke(packet, sourceEP);
        }

        private void NetworkReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint? EP = null;
            byte[] receiveBuffer = _listener.EndReceive(ar, ref EP);

            if (_receive)
                _listener.BeginReceive(NetworkReceiveCallback, null);

            NetworkPacket packet = new NetworkPacket(receiveBuffer);

            foreach (var service in _services)
                service.OnMessageReceivedRaw(packet, EP);
        }
    }
}
