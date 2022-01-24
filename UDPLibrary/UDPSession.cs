using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary.Packets;

namespace UDPLibrary
{
    public class UDPSession
    {
        public IPEndPoint endpoint;
        public uint sessionVersion;

        public UDPSession(OpenSessionRequestPacket openSessionRequest, IPEndPoint source)
        {
            endpoint = source;
            sessionVersion = openSessionRequest.sessionVersion;
        }
    }
}
