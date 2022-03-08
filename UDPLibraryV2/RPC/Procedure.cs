using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.RPC
{
    internal struct Procedure
    {
        public Func<IRequest, IResponse> proc;

        public Type responseType;
        public Type requestType;
    }
}
