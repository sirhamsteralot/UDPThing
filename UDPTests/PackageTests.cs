using Xunit;
using UDPLibrary.Packets;

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

            var networkPacket = PacketHelper.CreatePacket(packet, 0, false);

            Assert.Equal(36, networkPacket.payload.Length);

            var decompiledPacket = new CompositePacket();
            networkPacket.Deserialize(ref decompiledPacket);

            Assert.Single(decompiledPacket.subPackets);

            Assert.Equal("hello world", ((TestPacket)decompiledPacket.subPackets[0]).thisisavalue);
        }
    }
}