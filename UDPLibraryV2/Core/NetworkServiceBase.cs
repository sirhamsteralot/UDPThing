using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Core
{
    internal abstract class NetworkServiceBase : INetworkService
    {
        UDPCore _core;

        public NetworkServiceBase(UDPCore core)
        {
            _core = core;
        }

        public abstract void OnMessageReceivedRaw(NetworkPacket incomingPacket, IPEndPoint? sourceEndPoint);
    }
}
