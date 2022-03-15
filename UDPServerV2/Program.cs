using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UDPLibraryV2.Core;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.Core.PacketQueueing;
using UDPLibraryV2.Core.Serialization;
using UDPLibraryV2.Stats;
using UDPLibraryV2.RPC;
using UDPServerV2.ReqRes;
using UDPLibraryV2.RPC.Attributes;

namespace UDPServerV2
{
    internal class Program
    {
        static UDPCore core;

        struct StructWithSomeData
        {
            public int value1;
            public int value2;
            public double value3;
            public bool value4;

            public override string ToString()
            {
                return $"value1: {value1}, value2: {value2}, value3: {value3}, value4: {value4}";
            }
        }

        static async Task Main(string[] args)
        {
            var input = Console.ReadLine();
            var epTests = new EndpointTests();

            if (input == "s")
                epTests.StartServer();
            else if (input == "c")
                epTests.StartClient();
        }

        static void StartServer()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            core = new UDPCore(endPoint);

            core.StartListening();
            core.StartSending();
            core.OnPayloadReceivedEvent += Core_OnPayloadReceivedEvent;

            RPCService rpcService = new RPCService(core);
            rpcService.FindAndRegisterProcedures();

            Console.WriteLine("Ready. press any key to exit.");
            Console.ReadLine();
        }

        static async Task StartClient()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            core = new UDPCore(endPoint);
            core.StartListening();
            core.StartSending();
            core.OnPayloadReceivedEvent += Core_OnPayloadReceivedEvent;

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);

            StatTracker.Instance = new StatTracker((x) => Console.WriteLine(x.ToString()));

            RPCService rpcService = new RPCService(core);
            rpcService.FindAndRegisterProcedures();

            while (true)
            {
                Console.WriteLine("Send packet?");
                Console.ReadLine();

                StructWithSomeData dataStruct = new StructWithSomeData() {
                    value1 = 1,
                    value2 = 2,
                    value3 = 3,
                    value4 = true,
                };

                Guid guid = Guid.NewGuid();

                byte[] data = new byte[24];
                ValueSerializer.NetworkValueSerialize(guid, data, 0);
                Console.WriteLine($"sending...\n{BitConverter.ToString(data)}");

                try
                {
                    //var response = await rpcService.CallProcedure<HelloWorldRequest, HelloWorldResponse>(new HelloWorldRequest(), false, remoteEP, 2500);
                    //Console.WriteLine(response.ReturnedData);
                } catch (TimeoutException e)
                {
                    Console.WriteLine("Request timed out..");
                }

                //core.QueueUnmanaged(dataStruct, 12, true, SendPriority.Medium, remoteEP);
                //await core.SendUnmanagedReliable(guid, 12, true, remoteEP, 3, 2500).ContinueWith(x => Console.WriteLine($"Sent?: {x.Result}"));

                Console.WriteLine("packages sent: " + core.TotalPackagesSent);
            }
        }

        [Procedure(6, typeof(HelloWorldRequest), typeof(HelloWorldResponse))]
        public static IResponse ProcedureCall(IRequest request, IPEndPoint source)
        {
            return new HelloWorldResponse() { ReturnedData = "hello world" };
        }

        private static void Core_OnPayloadReceivedEvent(ReconstructedPacket packet, IPEndPoint? source)
        {
            Console.WriteLine("================| Received broadcast: |================");

            Console.WriteLine($"Packet: sId: {BitConverter.ToString(BitConverter.GetBytes(packet.StreamId))} T: {packet.TypeId} F: {packet.Flags}");
            Console.WriteLine($"Source IP: {source.Address}:{source.Port}");

            //Console.WriteLine($"Contents: {ValueSerializer.NetworkValueDeSerialize<Guid>(packet.GetPayloadBytes(), 0)}");

            Console.WriteLine($"===================> Full  packet <===================");
            Console.WriteLine($"{BitConverter.ToString(packet.GetPayloadBytes())}");

            Console.WriteLine("=======================================================");
        }
    }
}