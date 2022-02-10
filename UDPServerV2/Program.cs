using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UDPLibraryV2.Core;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.Core.PacketQueueing;
using UDPLibraryV2.Core.Serialization;

namespace UDPServerV2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var input = Console.ReadLine();

            if (input == "s")
                StartServer();
            else if (input == "c")
                StartClient();
        }

        static void StartServer()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            UDPCore core = new UDPCore(endPoint);

            core.StartListening();
            core.StartSending();
            core.OnPayloadReceivedEvent += Core_OnPayloadReceivedEvent;

            Console.WriteLine("Ready. press any key to exit.");
            Console.ReadLine();
        }

        static async void StartClient()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            UDPCore core = new UDPCore(endPoint);
            core.StartListening();
            core.StartSending();
            core.OnPayloadReceivedEvent += Core_OnPayloadReceivedEvent;

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            short stream = core.OpenStream(remoteEP);

            while (true)
            {
                Console.WriteLine("Send packet?");
                Console.ReadLine();

                var serializable = new RandomSerializable(512);
                Console.WriteLine($"sending...\n{BitConverter.ToString(serializable.bytes)}");

                core.QueueSerializable(serializable, stream, false, SendPriority.Medium);
            }
        }

        private static void Core_OnPayloadReceivedEvent(ReconstructedPacket packet, IPEndPoint? source)
        {
            Console.WriteLine("================| Received broadcast: |================");

            Console.WriteLine($"Packet: sId: {BitConverter.ToString(BitConverter.GetBytes(packet.StreamId))} T: {packet.TypeId} F: {packet.Flags}");
            Console.WriteLine($"Source IP: {source.Address}:{source.Port}");

            Console.WriteLine($"===================> Full  packet <===================");
            Console.WriteLine($"{BitConverter.ToString(packet.GetPayloadBytes())}");

            Console.WriteLine("=======================================================");
        }
    }
}