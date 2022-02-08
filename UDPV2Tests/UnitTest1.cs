using Xunit;
using UDPLibraryV2.Core;
using System.Net;

namespace UDPV2Tests
{
    public class UnitTest1
    {
        [Fact]
        public void CreateCoreTest()
        {
            IPEndPoint listenIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            UDPCore core = new UDPCore(listenIp);

            Assert.NotNull(core);
        }

        [Fact]
        public void SendTest()
        {
            IPEndPoint listenIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            UDPCore core = new UDPCore(listenIp);

            core.


        }
    }
}