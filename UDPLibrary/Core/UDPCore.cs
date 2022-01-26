using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using UDPLibrary.Packets;
using UDPLibrary.RUdp;

namespace UDPLibrary.Core
{
    public class UDPCore
    {
        public event Action<NetworkPacket, IPEndPoint>? OnPacketReceived;
        public event Action<NetworkPacket>? OnPacketFailedToSend;

        private int _listenPort;
        private uint _broadcastCount;
        private int _timeout;

        private bool _listen = false;
        private UdpClient _listener;

        private ReliablePacketTracker _packetTracker;
        private PacketBuffering _packetBuffering;

        private ConcurrentDictionary<int, NetworkPacket> _requestResponses;

        public UDPCore(int listenPort = 0, int maxRetries = 3, int timeout = 2500, int steadyPackageRate = 60)
        {
            _requestResponses = new ConcurrentDictionary<int, NetworkPacket>();

            _packetTracker = new ReliablePacketTracker(this, timeout, maxRetries);
            _timeout = timeout;

            _packetTracker.OnPacketFailedToSend += (x) => OnPacketFailedToSend?.Invoke(x);

            _listenPort = listenPort;
            _listener = new UdpClient(listenPort);

            _packetBuffering = new PacketBuffering(this, steadyPackageRate);
            _packetBuffering.StartBuffer();

            StartListening();
        }

        private void StartListening()
        {
            if (_listenPort == 0)
                _listenPort = ((IPEndPoint)_listener.Client.LocalEndPoint).Port;

            _listen = true;

            Receive();
        }

        public void BufferPacket(IPEndPoint endPoint, INetworkPacket packet, uint sessionId, byte sessionSeq)
        {
            _packetBuffering.QueuePacket(endPoint, packet, sessionId, sessionSeq);
        }

        public async Task SendPacketAsync(IPEndPoint endPoint, INetworkPacket packet, bool reliable, uint sessionId = 0, byte sessionSeq = 0)
        {
            NetworkPacket networkPacket = PacketHelper.CreatePacket(packet, _broadcastCount++, reliable, sessionId, sessionSeq);

            await SendPacketAsync(endPoint, networkPacket);
        }

        public async Task SendPacketAsync(IPEndPoint endPoint, NetworkPacket networkPacket)
        {
            var task = _listener.SendAsync(networkPacket.payload, networkPacket.payload.Length, endPoint);

            if (!networkPacket.reliablePacket)
                return;

            _packetTracker.TrackPacket(networkPacket, endPoint);

            await task;
        }

        public async Task<NetworkPacket> RequestResponseAsync(IPEndPoint endpoint, INetworkPacket outGoingPacket, bool reliable)
        {
            int streamId = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);

            var packedPacket = PacketHelper.CreatePacket(outGoingPacket, _broadcastCount, true, streamId);

            await SendPacketAsync(endpoint, packedPacket);

            for (int i = 0; i < _timeout; i += _timeout / 10)
            {
                await Task.Delay(_timeout / 100);

                if (_requestResponses.TryGetValue(streamId, out var responseByte))
                {
                    return responseByte;
                }
            }

            return null;
        }

        public void StopReceiving()
        {
            _listen = false;
            _listener.Client.Close();
        }

        private void Receive()
        {
            _listener.BeginReceive(MyReceiveCallback, null);
        }

        private unsafe void MyReceiveCallback(IAsyncResult result)
        {
            IPEndPoint? EP = null;
            byte[] bytes = _listener.EndReceive(result, ref EP);

            NetworkPacket packet = new NetworkPacket(bytes);

            if (packet.packetType == AckPacket.packetType)
                _packetTracker.OnPacketAcknowledged(packet);

            _requestResponses[packet.eventstreamId] = packet;

            if (EP != null)
            {
                OnPacketReceived?.Invoke(packet, EP);
                if (packet.reliablePacket)
                    AcknowledgeReliablePacket(EP, packet.packetId);
            }

            if (_listen)
            {
                Receive();
            }
        }

        private void AcknowledgeReliablePacket(IPEndPoint sourceEP, uint packetIndex)
        {
            var packet = PacketHelper.CreateAckPacket(packetIndex, _broadcastCount);
            SendPacketAsync(sourceEP, packet);
        }
    }
}