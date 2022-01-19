using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public class UDPEndpoint
    {
        public event Action<NetworkPacket, IPEndPoint> OnMessageReceived;

        private int _listenPort;
        private uint _broadCastCount;

        private bool _listen = false;
        private UdpClient _listener;
        private IPEndPoint _groupEP;

        private List<RemoteEndPoint> _endPoints;

        public UDPEndpoint()
        {
            _groupEP = new IPEndPoint(IPAddress.Any, _listenPort);
            _listener = new UdpClient();
            _endPoints = new List<RemoteEndPoint>();
        }

        public void StartListening(int listenPort)
        {
            _listenPort = listenPort;
            _listener = new UdpClient(_listenPort);
            _listen = true;

            Receive();
        }

        public void SendMessage(IPEndPoint endPoint, INetworkPacket packet, bool reliable)
        {
            NetworkPacket networkPacket = PacketFactory.CreatePacket(packet, _broadCastCount++, reliable);

            _listener.Send(networkPacket.payload, networkPacket.payload.Length, endPoint);
        }

        public void SendMessage(IPEndPoint endPoint, NetworkPacket networkPacket, bool reliable)
        {
            _listener.Send(networkPacket.payload, networkPacket.payload.Length, endPoint);
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

            if (EP != null)
                OnMessageReceived(packet, EP);

            if (_listen)
            {
                Receive();
            }
        }
    }
}