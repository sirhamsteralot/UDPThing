using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary;
using UDPLibrary.Packets;

namespace PerformanceTests
{
    public class UDPBenches
    {
        private TestPacket testPacket;

        private NetworkPacket networkTestPacket;

        private UDPCore udpEndpoint;
        private IPEndPoint ep;

        public UDPBenches()
        {
            testPacket = new TestPacket();
            testPacket.thisisavalue = "sdfasdfasfd";

            networkTestPacket = PacketFactory.CreatePacket(testPacket, 1, false);

            udpEndpoint = new UDPCore();
            IPAddress server = IPAddress.Parse("127.0.0.1");
            ep = new IPEndPoint(server, 11000);
        }

        [Benchmark]
        public void PacketFactoryBench()
        {
            PacketFactory.CreatePacket(testPacket, 1, false);
        }

        uint networkingVersion = 1;
        uint index = 0;
        bool reliable = false;

        [Benchmark]
        public unsafe void OverHeadBench()
        {
            byte[] payload = new byte[13];

            uint packetType = 0;
            

            fixed (byte* pbytes = &payload[0])
            {
                *(uint*)pbytes = networkingVersion;
                *(uint*)(pbytes + 4) = packetType;
                *(uint*)(pbytes + 8) = index;
                *(bool*)(pbytes + 12) = reliable;
            }

            var networkPacket = new NetworkPacket()
            {
                packetVersion = networkingVersion,
                packetId = index,
                packetType = packetType,
                reliablePacket = reliable,
            };

            networkPacket.payload = payload;
        }

        [Benchmark]
        public void SendMessageBench()
        {
            TestPacket packet = new TestPacket();
            packet.thisisavalue = "hello world!";

            udpEndpoint.SendMessageAsync(ep, packet, false);
        }
    }
}
