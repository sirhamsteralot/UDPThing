using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UDPLibrary;
using UDPLibrary.Events;

class Program
{
    private static byte[] buffer = new byte[4096];
    private static Socket s;
    private static UdpClient udpClient;
    private static IPEndPoint ep;

    private static int counter;

    static void Main(string[] args)
    {
        UDPServer uDPServer = new UDPServer();
        IPAddress server = IPAddress.Parse("127.0.0.1");
        ep = new IPEndPoint(server, 11000);

        while (true)
        {
            Console.WriteLine("input a thing");

            string arg = Console.ReadLine();
            TestPacket packet = new TestPacket();
            packet.thisisavalue = arg;

            NetworkPacket networkPacket = new NetworkPacket(1,1, counter++, false);
            networkPacket.Serialize(packet, packet.GetSize());

            uDPServer.SendMessage(ep, networkPacket);

            Console.WriteLine("Message sent to the broadcast address");
        }
    }
}