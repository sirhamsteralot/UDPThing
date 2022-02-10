using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;


namespace PerformanceTests
{
    public class RandomBenches
    {
        public long timeStamp;
        public Stopwatch _stopWatch = Stopwatch.StartNew();


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
        public void CheckAllocation()
        {
            NetworkPacket packet = new NetworkPacket(0, 2, 0);
        }
    }
}
