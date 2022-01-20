using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UDPLibrary;
using UDPLibrary.Packets;

class Program
{
    private static IPEndPoint ep;

    static void Main(string[] args)
    {
        UDPEndpoint udpEndpoint = new UDPEndpoint();
        IPAddress server = IPAddress.Parse("127.0.0.1");
        ep = new IPEndPoint(server, 11000);

        udpEndpoint.OnPacketFailedToSend += UdpEndpoint_OnPacketFailedToSend;

        while (true)
        {
            Console.WriteLine("input a thing");

            string arg = Console.ReadLine();
            TestPacket packet = new TestPacket();
            packet.thisisavalue = arg;

            udpEndpoint.SendMessage(ep, packet, true);

            Console.WriteLine("Message sent to the broadcast address");
        }
    }

    private static void UdpEndpoint_OnPacketFailedToSend(NetworkPacket obj)
    {
        Console.WriteLine($"packet: {obj.packetId} failed to send!");
    }
}