using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary;
using UDPLibrary.Packets;
using Xunit;

namespace UDPTests
{
    public class EndpointTests
    {

        public EndpointTests()
        {
        }

        [Fact]
        public void CreateServerTest()
        {
            UDPEndpoint server = new UDPEndpoint(11001);
            server.OnMessageReceived += OnReceive;
        }

        [Fact]
        public void CreateClientTest()
        {
            UDPEndpoint client = new UDPEndpoint();
            client.OnMessageReceived += OnReceive;
        }

        [Fact]
        public void SendMessageTest()
        {
            UDPEndpoint client = new UDPEndpoint();
            client.OnMessageReceived += OnReceive;

            IPAddress serverIP = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEp = new IPEndPoint(serverIP, 11000);

            TestPacket packet = new TestPacket();
            packet.thisisavalue = "Hello World";

            client = new UDPEndpoint();
            client.SendMessage(serverEp, packet, false);
        }

        [Fact]
        public void ReceiveTest()
        {
            TestPacket packet = new TestPacket();
            packet.thisisavalue = "Hello World";

            IPAddress serverIP = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEp = new IPEndPoint(serverIP, 11000);

            var networkpacket = PacketFactory.CreatePacket(packet, 0, false);

            OnReceive(networkpacket, serverEp);
        }

        private static void OnReceive(NetworkPacket incoming, IPEndPoint ep)
        {
            TestPacket packet = new TestPacket();
            incoming.Deserialize(ref packet);

            Assert.Equal("Hello World", packet.thisisavalue);
        }
    }
}
