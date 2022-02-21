using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Stats
{
    public class InternalFragmentTracker
    {
        public short FragmentId;

        public long _stopwatchOffset;
        public long _stopwatchEnd;

        public Stopwatch _stopwatch;

        public void End()
        {
            _stopwatchEnd = _stopwatch.ElapsedTicks;
        }

        public override string ToString()
        {
            return $"{FragmentId}, time: {_stopwatchOffset / (Stopwatch.Frequency / 1000000)} us to {(_stopwatchEnd) / (Stopwatch.Frequency / 1000000)} us.";
        }
    }
}
