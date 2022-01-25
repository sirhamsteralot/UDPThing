using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UDPLibrary;
using UDPLibrary.Packets;

class Program
{
    private static IPEndPoint ep;

    static async Task Main(string[] args)
    {
        UDPInterface udpEndpoint = new UDPInterface();
        IPAddress server = IPAddress.Parse("127.0.0.1");
        ep = new IPEndPoint(server, 11000);

        udpEndpoint.RawOnPacketFailedToSend += UdpEndpoint_OnPacketFailedToSend;

        Console.WriteLine("press enter to try open session");
        Console.ReadLine();
        Console.WriteLine("opening...");

        UDPSession session = await udpEndpoint.OpenSession(ep);

        if (session != null)
            Console.WriteLine("Session opened!");
        else
            Console.WriteLine("Session failed to open!");


        while (true)
        {
            Console.WriteLine("input a thing");

            string arg = Console.ReadLine();
            TestPacket packet = new TestPacket();
            packet.thisisavalue = arg;

            session.BufferPacket(packet);

            Console.WriteLine("Message sent to the broadcast address");
        }
    }

    private static void UdpEndpoint_OnPacketFailedToSend(NetworkPacket obj)
    {
        Console.WriteLine($"packet: {obj.packetId} failed to send!");
    }
}