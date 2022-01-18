using System;
using System.Net;
using System.Text;
using UDPLibrary;

namespace UDPThing
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UDPServer client = new UDPServer(11000);
            client.OnMessageReceived += OnReceive;

            while (true) ;
        }

        static void OnReceive(byte[] bytes, IPEndPoint ep)
        {
            Console.WriteLine($"Received broadcast");
            Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");
        }
    }
}
