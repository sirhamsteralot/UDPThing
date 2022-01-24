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
    public class UDPEndpoint
    {
        public event Action<NetworkPacket, IPEndPoint>? OnMessageReceived;
        public event Action<NetworkPacket>? OnPacketFailedToSend;

        private int _listenPort;
        private uint _broadCastCount;

        private bool _listen = false;
        private UdpClient _listener;

        private ReliablePacketTracker _packetTracker;

        public UDPEndpoint(int listenPort = 0, int maxRetries = 3, int timeout = 2500)
        {
            _packetTracker = new ReliablePacketTracker(this, timeout, maxRetries);

            _packetTracker.OnPacketFailedToSend += (x) => OnPacketFailedToSend?.Invoke(x);

            _listenPort = listenPort;
            _listener = new UdpClient(listenPort);

            StartListening();
        }

        private void StartListening()
        {
            if (_listenPort == 0)
                _listenPort = ((IPEndPoint)_listener.Client.LocalEndPoint).Port;

            _listen = true;

            Receive();
        }

        public void SendMessage(IPEndPoint endPoint, INetworkPacket packet, bool reliable)
        {
            NetworkPacket networkPacket = PacketFactory.CreatePacket(packet, _broadCastCount++, reliable);

            SendMessage(endPoint, networkPacket);
        }

        public void SendMessage(IPEndPoint endPoint, NetworkPacket networkPacket)
        {
            _ = _listener.SendAsync(networkPacket.payload, networkPacket.payload.Length, endPoint);

            if (!networkPacket.reliablePacket)
                return;

            _packetTracker.TrackPacket(networkPacket, endPoint);
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
            SendMessage(sourceEP, packet);
        }
    }
}