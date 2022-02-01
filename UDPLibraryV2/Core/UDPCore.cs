using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Core
{
    internal class UDPCore
    {
        private int _listenPort;
        private UdpClient _listener;

        public UDPCore(IPEndPoint listenIp)
        {
            _listener = new UdpClient(listenIp);
            _listenPort = ((IPEndPoint)_listener.Client.LocalEndPoint).Port;
        }

        public void StartListening()
        {
            _listener.BeginReceive(NetworkReceiveCallback, null);
        }

        private void NetworkReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint? EP = null;
            byte[] receiveBuffer = _listener.EndReceive(ar, ref EP);

            NetworkPacket packet = new NetworkPacket(receiveBuffer);


        }
    }
}
