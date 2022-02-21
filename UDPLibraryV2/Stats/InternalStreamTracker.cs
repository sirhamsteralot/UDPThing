using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Stats
{
    public class InternalStreamTracker
    {
        public Stopwatch stopwatch;

        public int PayloadSize;
        public int CompressedSize;

        public Dictionary<int, InternalFragmentTracker> FragmentTrackers { get; set; } = new Dictionary<int, InternalFragmentTracker> ();

        private Action<InternalStreamTracker> _callback;

        public InternalStreamTracker(Action<InternalStreamTracker> callback)
        {
            _callback = callback;
            stopwatch = Stopwatch.StartNew();
        }

        public void End()
        {
            stopwatch.Stop();
            _callback.Invoke(this);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Stream tracking ended: {FragmentTrackers.Count} fragments in {stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1000000)} us, {PayloadSize} ->>- {CompressedSize}");

            foreach (var fragment in FragmentTrackers)
            {
                stringBuilder.AppendLine(fragment.Value.ToString());
            }

            return stringBuilder.ToString();
        }

        public void AddFragment(PacketFragment fragment)
        {
            var fragmentTracker = new InternalFragmentTracker
            {
                _stopwatch = stopwatch,
                _stopwatchOffset = stopwatch.ElapsedTicks,
                FragmentId = fragment.FragmentId
            };

            FragmentTrackers.Add(fragment.FragmentId << 7 | fragment.FrameIndex, fragmentTracker);
        }

        public void EndFragment(PacketFragment fragment)
        {
            FragmentTrackers[fragment.FragmentId << 7 | fragment.FrameIndex].End();
        }
    }
}
