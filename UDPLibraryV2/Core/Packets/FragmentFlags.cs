using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Packets
{
    [Flags]
    internal enum FragmentFlags
    {
        TypeId = 1,
        Fragmented = 2,
        Compressed = 4,
    }
}
