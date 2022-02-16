﻿using System;
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

namespace UDPLibraryV2.Core
{
    public class UDPCore
    {
        public event Action<ReconstructedPacket, IPEndPoint?> OnPayloadReceivedEvent;
        public event Action<NetworkPacket, IPEndPoint?> OnMessageReceivedRaw;

        public readonly int _maximumPayloadSize;

        private bool _receive;
        private int _listenPort;
        private UdpClient _listener;

        private DeconstructionService _deconstructionService;
        private ReconstructionService _reconstructionService;

        private PacketSender _packetSender;

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

        public void QueueSerializable(INetworkSerializable serializable, bool compression, SendPriority priority, IPEndPoint remote)
        {
            byte[] serializationBuffer = new byte[serializable.MinimumBufferSize];

            serializable.Serialize(serializationBuffer, 0);

            var fragments = _deconstructionService.CreateFragments(serializationBuffer, serializable.TypeId, compression);
            foreach (var fragment in fragments)
            {
                QueueFragment(fragment, priority, remote);
            }
        }

        public async Task<bool> SendSerializableReliable(INetworkSerializable serializable, bool compression, IPEndPoint remote, int retries, int retryDelay)
        {
            byte[] serializationBuffer = ArrayPool<byte>.Shared.Rent(serializable.MinimumBufferSize);
            byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(_maximumPayloadSize);

            serializable.Serialize(serializationBuffer, 0);

            var fragments = _deconstructionService.CreateFragments(serializationBuffer, serializable.TypeId, compression);

            short streamId = (short)Random.Shared.Next();

            Dictionary<byte, NetworkPacket> sentPackets = new Dictionary<byte, NetworkPacket>(serializable.MinimumBufferSize + _maximumPayloadSize - 1 / _maximumPayloadSize);

            TaskCompletionSource<bool> completed = new TaskCompletionSource<bool>();
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Action<NetworkPacket, IPEndPoint?> messageReceivedCallback = (packet, ep) =>
            {
                if (packet.Streamid == streamId && ((packet.Flags | PacketFlags.Acknowledge) == PacketFlags.Acknowledge))
                {
                    if (sentPackets.Remove(packet.Seq))
                    {
                        if (sentPackets.Count < 1)
                        {
                            completed.SetResult(true);
                            tokenSource.Cancel();
                        }
                    }
                }
            };

            OnMessageReceivedRaw += messageReceivedCallback;
            byte seq = 0;

            foreach (var fragment in fragments)
            {
                NetworkPacket packet = new NetworkPacket(PacketFlags.Reliable, seq, streamId);
                packet.AddFragment(fragment);

                await SendPacketAsync(packet, remote, sendBuffer);

                sentPackets.Add(packet.Seq, packet);
                seq++;
            }

            while (retries-- > 0 && !completed.Task.IsCompleted)
            {
                try
                {
                    await Task.Delay(retryDelay, tokenSource.Token);
                } catch (TaskCanceledException)
                {
                    break;
                }

                foreach (var packet in sentPackets.Values)
                {
                    await SendPacketAsync(packet, remote, sendBuffer);
                }
            }

            if (sentPackets.Count > 0)
                completed.TrySetResult(false);

            bool result = await completed.Task;

            // Cleanup
            OnMessageReceivedRaw -= messageReceivedCallback;
            ArrayPool<byte>.Shared.Return(sendBuffer);
            ArrayPool<byte>.Shared.Return(serializationBuffer);

            return result;
        }

        internal void RegisterService(INetworkService service)
        {
            _services.Add(service);
        }

        internal async Task SendPacketAsync(NetworkPacket packet, IPEndPoint remote, byte[] sendBuffer = null)
        {
            if (sendBuffer == null)
                sendBuffer = ArrayPool<byte>.Shared.Rent(_maximumPayloadSize);

            int bytesSerialized = packet.SerializeToBuffer(sendBuffer);
            await SendBytesAsync(sendBuffer, bytesSerialized, remote);

            ArrayPool<byte>.Shared.Return(sendBuffer);
        }

        internal async Task SendBytesAsync(byte[] bytes, int amountToSend, IPEndPoint endPoint)
        {
            await _listener.SendAsync(bytes, amountToSend, endPoint);
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

                SendPacketAsync(ackPacket, EP).ContinueWith(x => throw x.Exception, TaskContinuationOptions.OnlyOnFaulted);
            }

            foreach (var service in _services)
                service.OnMessageReceivedRaw(packet, EP);

            OnMessageReceivedRaw?.Invoke(packet, EP);
        }
    }
}
