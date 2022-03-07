using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Stats
{
    public class StatTracker
    {
        public static StatTracker Instance { get; set; } = null;

        Action<InternalStreamTracker> _streamCallback;

        public StatTracker (Action<InternalStreamTracker> streamCallback)
        {
            _streamCallback = streamCallback;
        }

        public InternalStreamTracker CreateNewMessageTracker()
        {
            return new InternalStreamTracker(_streamCallback);
        }
    }
}
