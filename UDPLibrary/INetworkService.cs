using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public interface INetworkService
    {
        public event Action<IPEndPoint, INetworkPacket, bool>? immediatePacketSendRequestEvent;
        public event Action<IPEndPoint, INetworkPacket>? packetQueueEvent;

        public void OnPacketReceived(NetworkPacket packet, IPEndPoint source);
        public void OnPacketFailedToSend(NetworkPacket packet);

        public void InjectSessions(ConcurrentDictionary<IPEndPoint, UDPSession> sessions);
    }
}
