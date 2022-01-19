using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public class ClientEndPoint
    {
        public IPEndPoint clientEp;
        public UdpClient client;

        public ClientEndPoint(IPEndPoint clientEp, UdpClient client, byte[] bytes)
        {
            this.clientEp = clientEp;
            this.client = client;
        }

        public void SendMessage(byte[] message)
        {
            client.Send(message, message.Length, clientEp);
        }
    }
}
