using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public class RemoteEndPoint
    {
        public IPEndPoint clientEp;
        public UdpClient client;
        public uint packetCount;

        public RemoteEndPoint(IPEndPoint clientEp, UdpClient client, byte[] bytes)
        {
            this.clientEp = clientEp;
            this.client = client;
        }

        public void SendMessage(IPEndPoint endPoint, INetworkPacket packet, bool reliable)
        {
            NetworkPacket networkPacket = PacketFactory.CreatePacket(packet, packetCount++, reliable);

            client.Send(networkPacket.payload, networkPacket.payload.Length, endPoint);
        }
    }
}
