using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.Core;
using System.Net;
using System.Buffers;
using UDPLibraryV2.Core.PacketQueueing;
using UDPLibraryV2.Utils;
using System.Security.Cryptography;

namespace PerformanceTests
{
    public class RandomBenches
    {
        public long timeStamp;
        public Stopwatch _stopWatch = Stopwatch.StartNew();
        public UDPCore core = new UDPCore(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
        public RandomSerializable toSerialize = new RandomSerializable(128);
        public IPEndPoint remote = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);

        ObjectPool<NetworkPacket> _networkPacketPool;
        ObjectPoolList<NetworkPacket> _networkPacketPoolList;

        public RandomBenches()
        {
            _networkPacketPool = new ObjectPool<NetworkPacket>(() => new NetworkPacket(), 3);
            _networkPacketPoolList = new ObjectPoolList<NetworkPacket>(() => new NetworkPacket(), 3);

            //minor warmup

            var array1 = ArrayPool<byte>.Shared.Rent(512);
            var array2 = ArrayPool<byte>.Shared.Rent(512);
            var array3 = ArrayPool<byte>.Shared.Rent(512);

            ArrayPool<byte>.Shared.Return(array1);
            ArrayPool<byte>.Shared.Return(array2);
            ArrayPool<byte>.Shared.Return(array3);
        }

        [Benchmark]
        public void CheckSendingReliableAllocation()
        {
            _ = core.SendSerializableReliable(toSerialize, false, remote, 0, 1).GetAwaiter().GetResult();
        }
    }
}
