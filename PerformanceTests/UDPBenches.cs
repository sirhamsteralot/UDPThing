using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary.Core;
using UDPLibrary.Packets;

namespace PerformanceTests
{
    public class UDPBenches
    {
        private TestPacket testPacket;

        private NetworkPacket networkTestPacket;

        private UDPCore udpEndpoint;
        private IPEndPoint ep;

        private byte[] toDecompress;

        public UDPBenches()
        {
            testPacket = new TestPacket();
            testPacket.thisisavalue = "sdfasdfasfd";

            networkTestPacket = PacketHelper.CreatePacket(testPacket, 1, false);

            udpEndpoint = new UDPCore();
            IPAddress server = IPAddress.Parse("127.0.0.1");
            ep = new IPEndPoint(server, 11000);

            toDecompress = Compression.Compress(networkTestPacket.payload);
        }

        [Benchmark]
        public void PacketFactoryBench()
        {
            PacketHelper.CreatePacket(testPacket, 1, false);
        }

        uint networkingVersion = 1;
        uint index = 0;
        bool reliable = false;

        [Benchmark]
        public void SendMessageBench()
        {
            TestPacket packet = new TestPacket();
            packet.thisisavalue = "hello world!";

            udpEndpoint.SendPacketAsync(ep, packet, false, 0).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void CompressBench()
        {
            var compressed = Compression.Compress(networkTestPacket.payload);
        }

        [Benchmark]
        public void DecompressBench()
        {
            var decompressed = Compression.Decompress(toDecompress);
        }
    }
}
