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

        private static void OnReceive(NetworkPacket incoming, IPEndPoint ep)
        {
            Console.WriteLine("Received broadcast:");
            TestPacket packet = new TestPacket();
            incoming.Deserialize(ref packet);

            Console.WriteLine($"Packet: V: {incoming.packetVersion} T: {incoming.packetType} I: {incoming.packetId} R: {incoming.reliablePacket}");
            Console.WriteLine($"Full packet: {BitConverter.ToString(incoming.payload)}");

            Console.WriteLine(packet.thisisavalue);
        }
    }
}
