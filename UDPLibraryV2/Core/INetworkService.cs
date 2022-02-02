using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Core
{
    internal interface INetworkService
    {
        public void OnMessageReceivedRaw(NetworkPacket incomingPacket, IPEndPoint? sourceEndPoint);
    }
}
