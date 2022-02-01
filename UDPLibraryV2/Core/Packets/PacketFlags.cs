using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Packets
{
    [Flags]
    internal enum PacketFlags
    {
        Acknowledge = 1,
        Reliable = 2,
        Compressed = 4,
    }
}
