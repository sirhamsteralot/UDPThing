using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.RPC
{
    internal struct ProcedureRecord
    {
        public Func<IRequest, IPEndPoint, IResponse> proc;

        public Type responseType;
        public Type requestType;
    }
}
