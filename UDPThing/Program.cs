using System;
using System.Collections.Generic;
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
            client.OnSessionOpened += Client_OnSessionOpened;

            Console.WriteLine("Ready.");
            while (true)
            {
                Console.ReadLine();
                GC.Collect();
                Console.WriteLine("GC executed");
            }
        }

        private static void Client_OnSessionOpened(UDPSession obj)
        {
            obj.OnCompositePacketReceived += OnCompositeReceived;
        }

        private static void OnReceive(NetworkPacket incoming, IPEndPoint ep)
        {

            //Console.WriteLine("====| Received broadcast: |====");

            //Console.WriteLine($"    Packet: V: {incoming.packetVersion} T: {incoming.packetType} I: {incoming.packetId} R: {incoming.reliablePacket}");
            //Console.WriteLine($"    Full packet: {BitConverter.ToString(incoming.payload)}");

            //Console.WriteLine("===============================");

            if (incoming.packetType != TestPacket.PacketType)
                return;
        }

        private static void OnCompositeReceived(List<INetworkPacket> packets, UDPSession session)
        {
            foreach (var packet in packets)
            {
                TestPacket testpacket = packet as TestPacket;

                if (testpacket != null)
                    Console.WriteLine(testpacket.thisisavalue);
            }
        }
    }
}
