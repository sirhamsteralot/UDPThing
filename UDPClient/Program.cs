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
        UDPInterface udpEndpoint = new UDPInterface();
        IPAddress server = IPAddress.Parse("127.0.0.1");
        ep = new IPEndPoint(server, 11000);

        udpEndpoint.RawOnPacketFailedToSend += UdpEndpoint_OnPacketFailedToSend;

        udpEndpoint.OpenSession(ep);

        while (true)
        {
            Console.WriteLine("input a thing");

            string arg = Console.ReadLine();
            TestPacket packet = new TestPacket();
            packet.thisisavalue = arg;

            //udpEndpoint.SendMessage(ep, packet, true);
            udpEndpoint.SendImmediateReliable(packet, ep);

            Console.WriteLine("Message sent to the broadcast address");
        }
    }

    private static void UdpEndpoint_OnPacketFailedToSend(NetworkPacket obj)
    {
        Console.WriteLine($"packet: {obj.packetId} failed to send!");
    }
}