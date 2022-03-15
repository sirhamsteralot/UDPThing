using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.EndPoint
{
    [Flags]
    public enum Permissions
    {
        None = 0,
        ConnectionVersion = 1,
        Base = 2,
    }
}
