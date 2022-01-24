using Xunit;
using UDPLibrary.Packets;
using UDPLibrary;

namespace UDPTests
{
    public class PackageTests
    {
        [Fact]
        public void TestCompositeSerialization()
        {
            CompositePacket packet = new CompositePacket();
            TestPacket subPacket = new TestPacket();
            subPacket.thisisavalue = "hello world";
            packet.AddPacket(subPacket);

            var networkPacket = PacketFactory.CreatePacket(packet, 0, false);

            Assert.Equal(36, networkPacket.payload.Length);

            var decompiledPacket = new CompositePacket();
            networkPacket.Deserialize(ref decompiledPacket);

            Assert.Equal(1, decompiledPacket.subPackets.Count);

            Assert.Equal("hello world", ((TestPacket)decompiledPacket.subPackets[0]).thisisavalue);
        }
    }
}