using System;
using System.Net;
using System.Text;
using UDPLibrary;
using UDPLibrary.Events;

namespace UDPThing
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UDPServer client = new UDPServer();
            client.StartListening(11000);
            client.OnMessageReceived += OnReceive;

            while (true) ;
        }

        private static void OnReceive(NetworkPacket arg1, IPEndPoint arg2)
        {
            Console.WriteLine("received broadcast");
            TestPacket packet = new TestPacket();
            arg1.Deserialize(ref packet);

            Console.WriteLine(packet.thisisavalue);
        }
    }
}
