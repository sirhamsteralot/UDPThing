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
        public IPEndPoint endPoint;
        public UdpClient client;

        public ClientEndPoint(IPEndPoint endPoint, UdpClient client)
        {
            this.endPoint = endPoint;
            this.client = client;
        }

        public void SendMessage(byte[] message)
        {
            client.Send(message, message.Length, endPoint);
        }
    }
}
