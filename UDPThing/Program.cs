using System;
using System.Net;
using System.Text;
using UDPLibrary;
using UDPLibrary.Packets;

namespace UDPThing
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UDPEndpoint client = new UDPEndpoint(11000);
            client.OnMessageReceived += OnReceive;

            while (true) ;
        }

        private static void OnReceive(NetworkPacket arg1, IPEndPoint arg2)
        {
            Console.WriteLine("Received broadcast:");
            TestPacket packet = new TestPacket();
            arg1.Deserialize(ref packet);

            Console.WriteLine($"Packet: V: {arg1.packetVersion} T: {arg1.packetType} I: {arg1.packetIndex} R: {arg1.reliablePacket}");
            Console.WriteLine($"Full packet: {BitConverter.ToString(arg1.payload)}");

            Console.WriteLine(packet.thisisavalue);
        }
    }
}
