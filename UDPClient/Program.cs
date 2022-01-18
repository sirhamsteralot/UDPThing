using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    private static byte[] buffer = new byte[4096];
    private static Socket s;
    private static UdpClient udpClient;
    private static IPEndPoint ep;

    static void Main(string[] args)
    {
        s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress server = IPAddress.Parse("127.0.0.1");
        ep = new IPEndPoint(server, 11000);
        udpClient = new UdpClient();

        Console.WriteLine("Everything ready?");
        Console.ReadLine();

        byte[] establish = new byte[] { 0, 1};
        s.SendTo(establish, ep);

        while (true)
        {
            Console.WriteLine("input a thing");

            string arg = Console.ReadLine();

            byte[] sendbuf = Encoding.ASCII.GetBytes(arg);
            udpClient.Send(sendbuf, ep);

            Console.WriteLine("Message sent to the broadcast address");
        }
    }
}