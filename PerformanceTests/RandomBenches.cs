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

namespace PerformanceTests
{
    public class RandomBenches
    {
        public long timeStamp;
        public Stopwatch _stopWatch = Stopwatch.StartNew();
        public UDPCore core = new UDPCore(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
        public RandomSerializable toSerialize = new RandomSerializable(128);
        public IPEndPoint remote = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);

        public RandomBenches()
        {
            var array1 = ArrayPool<byte>.Shared.Rent(512);
            var array2 = ArrayPool<byte>.Shared.Rent(512);
            var array3 = ArrayPool<byte>.Shared.Rent(512);

            ArrayPool<byte>.Shared.Return(array1);
            ArrayPool<byte>.Shared.Return(array2);
            ArrayPool<byte>.Shared.Return(array3);
        }


        [Benchmark]
        public void StopwatchBench()
        {
            _stopWatch = Stopwatch.StartNew();

            long elapsedMs = _stopWatch.ElapsedMilliseconds;
        }

        [Benchmark]
        public void TimeStampBench()
        {
            timeStamp = _stopWatch.ElapsedMilliseconds;

            long elapsedMs = _stopWatch.ElapsedMilliseconds - timeStamp;
        }

        [Benchmark]
        public void CheckPacketAllocation()
        {
            NetworkPacket packet = new NetworkPacket(0, 2, 0);
        }

        [Benchmark]
        public void CheckSendingReliableAllocation()
        {
            _ = core.SendSerializableReliable(toSerialize, false, remote, 0, 1).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void ArrayPoolCheck()
        {
            byte[] array = ArrayPool<byte>.Shared.Rent(512);

            ArrayPool<byte>.Shared.Return(array);
        }

        [Benchmark]
        public void ArrayAllocCheck()
        {
            byte[] array = new byte[512];
        }
    }
}
