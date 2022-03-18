using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.EndPoint
{
    public partial class RemoteEndPoint
    {
        public Permissions permissions;
        public IPEndPoint ip;

        private HashSet<short> availableProcedures;

        public RemoteEndPoint(Permissions permissions, IPEndPoint ip, HashSet<short> availableProcedures)
        {
            this.permissions = permissions;
            this.ip = ip;
            this.availableProcedures = availableProcedures;
        }
    }
}
