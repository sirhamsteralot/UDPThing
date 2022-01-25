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
            UDPInterface client = new UDPInterface(11000);
            client.RawOnPacketReceived += OnReceive;

            Console.WriteLine("Ready.");
            while (true) ;
        }

        private static void OnReceive(NetworkPacket incoming, IPEndPoint ep)
        {
            Console.WriteLine("====| Received broadcast: |====");

            Console.WriteLine($"    Packet: V: {incoming.packetVersion} T: {incoming.packetType} I: {incoming.packetId} R: {incoming.reliablePacket}");
            Console.WriteLine($"    Full packet: {BitConverter.ToString(incoming.payload)}");

            Console.WriteLine("===============================");

            if (incoming.packetType != TestPacket.PacketType)
                return;

            TestPacket packet = new TestPacket();
            incoming.Deserialize(ref packet);

            Console.WriteLine(packet.thisisavalue);
        }
    }
}
