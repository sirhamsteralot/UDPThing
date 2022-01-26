using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary.Packets;

namespace UDPLibrary.Services
{
    public interface INetworkService
    {
        public void OnPacketReceived(NetworkPacket packet, IPEndPoint source);
        public void OnPacketFailedToSend(NetworkPacket packet);

        public void InjectSessions(ConcurrentDictionary<IPEndPoint, UDPSession> sessions);
    }
}
