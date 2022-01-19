using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UDPLibrary.Packets;

namespace UDPLibrary
{
    public class UDPEndpoint
    {
        public event Action<NetworkPacket, IPEndPoint> OnMessageReceived;

        private int _listenPort;
        private uint _broadCastCount;

        private bool _listen = false;
        private UdpClient _listener;

        private List<RemoteEndPoint> _endPoints;

        private Dictionary<uint, Timer> _awaitingAcknowledgement;
        private int timeOut;

        public UDPEndpoint()
        {
            _awaitingAcknowledgement = new Dictionary<uint, Timer>();

            _listener = new UdpClient(0);
            _endPoints = new List<RemoteEndPoint>();

            var freePort = ((IPEndPoint)_listener.Client.LocalEndPoint).Port;
            StartListening(freePort);
        }

        public UDPEndpoint(int listenPort)
        {
            _awaitingAcknowledgement = new Dictionary<uint, Timer>();

            _listener = new UdpClient(listenPort);
            _endPoints = new List<RemoteEndPoint>();

            StartListening(listenPort);
        }

        private void StartListening(int listenPort)
        {
            _listenPort = listenPort;
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

            _awaitingAcknowledgement[networkPacket.packetIndex] = new Timer((x) => { SendMessage(endPoint, networkPacket); }, null, timeOut, Timeout.Infinite);
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
                Console.WriteLine("Acked");

            if (EP != null)
            {
                OnMessageReceived?.Invoke(packet, EP);
                if (packet.reliablePacket)
                    AcknowledgeReliablePacket(EP, packet.packetIndex);
            }

            if (_listen)
            {
                Receive();
            }
        }

        private void AcknowledgeReliablePacket(IPEndPoint sourceEP, uint packetIndex)
        {
            var packet = PacketFactory.CreatePacket(new AckPacket(packetIndex), _broadCastCount, false);
            SendMessage(sourceEP, packet);
        }
    }
}