using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.PacketQueueing
{
    internal class SendTarget
    {
        public IPEndPoint endPoint;
        public byte sequenceByte;

        public SendTarget(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }
    }
}
