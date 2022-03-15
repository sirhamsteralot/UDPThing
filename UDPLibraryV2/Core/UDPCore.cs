using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.PacketQueueing;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.Core.Serialization;
using UDPLibraryV2.Stats;

namespace UDPLibraryV2.Core
{
    public class UDPCore
    {
        public event Action<ReconstructedPacket, IPEndPoint?> OnPayloadReceivedEvent;
        public event Action<NetworkPacket, IPEndPoint?> OnMessageReceivedRaw;

        public long TotalPackagesSent { get; private set; }

        public readonly int _maximumPayloadSize;

        private bool _receive;
        private int _listenPort;
        private UdpClient _listener;

        private DeconstructionService _deconstructionService;
        private ReconstructionService _reconstructionService;

        private PacketSender _packetSender;
        private ReliablePacketSender _reliablePacketSender;

        private List<INetworkService> _services;

        public UDPCore(IPEndPoint listenIp, int sendRate = 64, int maxPayloadSize = 512)
        {
            _maximumPayloadSize = maxPayloadSize;

            _listener = new UdpClient(listenIp);
            _listenPort = ((IPEndPoint)_listener.Client.LocalEndPoint).Port;

            _services = new List<INetworkService>();

            _deconstructionService = new DeconstructionService(_maximumPayloadSize - NetworkPacket.HeaderSize);

            _reconstructionService = new ReconstructionService(this);
            _reconstructionService.OnPayloadReconstructed += ReconstructionService_OnPayloadReconstructed;
            RegisterService(_reconstructionService);

            _packetSender = new PacketSender(this, _maximumPayloadSize, sendRate);
            _reliablePacketSender = new ReliablePacketSender(this, _deconstructionService, maxPayloadSize);
        }


        public void StartListening()
        {
            _receive = true;
            _listener.BeginReceive(NetworkReceiveCallback, null);
        }

        public void StartSending()
        {
            _packetSender.StartSending();
        }

        public unsafe void QueueUnmanaged<T>(T value, short typeId, bool compression, SendPriority priority, IPEndPoint remote) where T: unmanaged
        {
            byte[] serializationBuffer = new byte[sizeof(T)];

            ValueSerializer.NetworkValueSerialize(value, serializationBuffer, 0);

            QueueBytes(serializationBuffer, typeId, compression, priority, remote);
        }

        public void QueueSerializable(INetworkSerializable serializable, bool compression, SendPriority priority, IPEndPoint remote)
        {
            byte[] serializationBuffer = new byte[serializable.RequiredSendBufferSize];

            serializable.Serialize(serializationBuffer, 0);

            QueueBytes(serializationBuffer, serializable.TypeId, compression, priority, remote);
        }

        public Task<bool> SendUnmanagedReliable<T>(T value, short typeId, bool compression, IPEndPoint remote, int retries, int retryDelay) where T: unmanaged
        {
            InternalStreamTracker messageTracker = StatTracker.Instance?.CreateNewMessageTracker();

            return _reliablePacketSender.SendUnmanagedReliable(value, typeId, compression, remote, retries, retryDelay, messageTracker);
        }

        public Task<bool> SendSerializableReliable(INetworkSerializable serializable, bool compression, IPEndPoint remote, int retries, int retryDelay)
        {
            InternalStreamTracker messageTracker = StatTracker.Instance?.CreateNewMessageTracker();

            return _reliablePacketSender.SendSerializableReliable(serializable, compression, remote, retries, retryDelay, messageTracker);
        }

        public void QueueBytes(byte[] bytes, short typeId, bool compression, SendPriority priority, IPEndPoint remote)
        {
            var fragments = _deconstructionService.CreateFragments(bytes, typeId, compression);
            foreach (var fragment in fragments)
            {
                QueueFragment(fragment, priority, remote);
            }
        }

        internal void RegisterService(INetworkService service)
        {
            _services.Add(service);
        }

        internal async ValueTask SendPacketAsync(NetworkPacket packet, IPEndPoint remote, byte[] sendBuffer = null)
        {
            try
            {
                if (sendBuffer == null)
                    sendBuffer = ArrayPool<byte>.Shared.Rent(_maximumPayloadSize);

                int bytesSerialized = packet.SerializeToBuffer(sendBuffer);
                await SendBytesAsync(sendBuffer, bytesSerialized, remote);

                ArrayPool<byte>.Shared.Return(sendBuffer);
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        internal Task<int> SendBytesAsync(byte[] bytes, int amountToSend, IPEndPoint endPoint)
        {
            try
            {
                TotalPackagesSent++;
                return _listener.SendAsync(bytes, amountToSend, endPoint);
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        internal void QueueFragment(in PacketFragment fragment, SendPriority priority, IPEndPoint remote)
        {
            _packetSender.QueueFragment(fragment, priority, remote);
        }

        private void ReconstructionService_OnPayloadReconstructed(ReconstructedPacket packet, IPEndPoint? sourceEP)
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

            if ((packet.Flags | PacketFlags.Reliable) == PacketFlags.Reliable)
            {
                NetworkPacket ackPacket = new NetworkPacket(PacketFlags.Acknowledge, packet.Seq, packet.Streamid);

                SendPacketAsync(ackPacket, EP);
            }

            foreach (var service in _services)
                service.OnMessageReceivedRaw(packet, EP);

            OnMessageReceivedRaw?.Invoke(packet, EP);
        }
    }
}
