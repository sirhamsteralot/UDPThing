using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary.Core;
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
            UDPCore server = new UDPCore(11001);
            server.OnPacketReceived += OnReceive;
        }

        [Fact]
        public void CreateClientTest()
        {
            UDPCore client = new UDPCore();
            client.OnPacketReceived += OnReceive;
        }

        [Fact]
        public void SendMessageTest()
        {
            UDPCore client = new UDPCore();
            client.OnPacketReceived += OnReceive;

            IPAddress serverIP = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEp = new IPEndPoint(serverIP, 11000);

            TestPacket packet = new TestPacket();
            packet.thisisavalue = "Hello World";

            client = new UDPCore();
            client.SendPacketAsync(serverEp, packet, false);
        }

        [Fact]
        public void ReceiveTest()
        {
            TestPacket packet = new TestPacket();
            packet.thisisavalue = "Hello World";

            IPAddress serverIP = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEp = new IPEndPoint(serverIP, 11000);

            var networkpacket = PacketHelper.CreatePacket(packet, 0, false);

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
