using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UDPLibraryV2.Core;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.Core.PacketQueueing;
using UDPLibraryV2.Core.Serialization;
using UDPLibraryV2.Stats;

namespace UDPServerV2
{
    internal class Program
    {
        static UDPCore core;
        static bool AMCLIENT;

        struct StructWithSomeData
        {
            public int value1;
            public int value2;
            public double value3;
            public bool value4;
        }

        static async Task Main(string[] args)
        {
            var input = Console.ReadLine();

            if (input == "s")
                StartServer();
            else if (input == "c")
                await StartClient();
        }

        static void StartServer()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            core = new UDPCore(endPoint);

            core.StartListening();
            core.StartSending();
            core.OnPayloadReceivedEvent += Core_OnPayloadReceivedEvent;

            Console.WriteLine("Ready. press any key to exit.");
            Console.ReadLine();
        }

        static async Task StartClient()
        {
            AMCLIENT = true;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            core = new UDPCore(endPoint);
            core.StartListening();
            core.StartSending();
            core.OnPayloadReceivedEvent += Core_OnPayloadReceivedEvent; 

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);

            StatTracker.Instance = new StatTracker((x) => Console.WriteLine(x.ToString()));

            while (true)
            {
                Console.WriteLine("Send packet?");
                Console.ReadLine();

                var serializable = new RandomSerializable(16);
                //Console.WriteLine($"sending...\n{BitConverter.ToString(serializable.bytes)}");

                //for (int i = 0; i < 1000; i++)
                //    core.QueueSerializable(serializable, true, SendPriority.Medium, remoteEP);

                StructWithSomeData dataStruct = new StructWithSomeData() {
                    value1 = 1,
                    value2 = 2,
                    value3 = 3,
                    value4 = true,
                };

                byte[] data = new byte[17];
                ValueSerializer.NetworkValueSerialize(dataStruct, data, 0);
                Console.WriteLine($"sending...\n{BitConverter.ToString(data)}");

                core.QueueUnmanaged(dataStruct, false, SendPriority.Medium, remoteEP);

                Console.ReadLine();
                Console.WriteLine("packages sent: " + core.TotalPackagesSent);

                //_ = core.SendSerializableReliable(serializable, true, remoteEP, 3, 2500).ContinueWith(x => Console.WriteLine($"Sent?: {x.Result}"));
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