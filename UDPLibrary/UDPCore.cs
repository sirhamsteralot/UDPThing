using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UDPLibrary.Packets;
using UDPLibrary.RUdp;

namespace UDPLibrary
{
    public class UDPCore
    {
        public event Action<NetworkPacket, IPEndPoint>? OnMessageReceived;
        public event Action<NetworkPacket>? OnPacketFailedToSend;

        private int _listenPort;
        private uint _broadCastCount;

        private bool _listen = false;
        private UdpClient _listener;

        private ReliablePacketTracker _packetTracker;
        private PacketBuffering _packetBuffering;

        public UDPCore(int listenPort = 0, int maxRetries = 3, int timeout = 2500, int steadyPackageRate = 60)
        {
            _packetTracker = new ReliablePacketTracker(this, timeout, maxRetries);

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

        public void BufferMessage(IPEndPoint endPoint, INetworkPacket packet)
        {
            _packetBuffering.QueuePacket(endPoint, packet);
        }

        public async Task SendMessageAsync(IPEndPoint endPoint, INetworkPacket packet, bool reliable)
        {
            NetworkPacket networkPacket = PacketFactory.CreatePacket(packet, _broadCastCount++, reliable);

            await SendMessageAsync(endPoint, networkPacket);
        }

        public async Task SendMessageAsync(IPEndPoint endPoint, NetworkPacket networkPacket)
        {
            var task = _listener.SendAsync(networkPacket.payload, networkPacket.payload.Length, endPoint);

            if (!networkPacket.reliablePacket)
                return;

            _packetTracker.TrackPacket(networkPacket, endPoint);

            await task;
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

        private void MyReceiveCallback(IAsyncResult result)
        {
            IPEndPoint? EP = null;
            byte[] bytes = _listener.EndReceive(result, ref EP);

            NetworkPacket packet = new NetworkPacket(bytes);

            if (packet.packetType == AckPacket.packetType)
                _packetTracker.OnPacketAcknowledged(packet);

            if (EP != null)
            {
                OnMessageReceived?.Invoke(packet, EP);
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
            var packet = PacketFactory.CreateAckPacket(packetIndex, _broadCastCount);
            SendMessageAsync(sourceEP, packet);
        }
    }
}