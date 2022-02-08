using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceTests
{
    public class SocketVsClient
    {
        Socket socket;
        UdpClient udpClient;
        IPEndPoint myEP;
        IPEndPoint remotEp;

        byte[] buffer;

        public SocketVsClient()
        {
            myEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            remotEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11001);

            socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            //socket.Bind(myEP);

            udpClient = new UdpClient(myEP);

            buffer = new byte[512];

            RandomNumberGenerator.Fill(buffer);
        }

        [Benchmark]
        public void SocketSendBenchmark()
        {
            socket.SendTo(buffer, remotEp);
        }

        [Benchmark]
        public void ClientSendBenchmark()
        {
            udpClient.Send(buffer, remotEp);
        }
    }
}
