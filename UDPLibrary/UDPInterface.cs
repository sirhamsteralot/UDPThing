using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary.Packets;

namespace UDPLibrary
{
    public class UDPInterface
    {
        public List<UDPSession> sessions;

        public List<INetworkService> services;
        public UDPCore udpEndpoint;

        public event Action<NetworkPacket, IPEndPoint>? RawOnMessageReceived;
        public event Action<NetworkPacket>? RawOnPacketFailedToSend;

        public UDPInterface(int listenPort = 0, int steadyPackageRate = 60, int maxRetries = 3, int timeout = 2500)
        {
            sessions = new List<UDPSession>();
            services = new List<INetworkService>();
            udpEndpoint = new UDPCore(listenPort, maxRetries, timeout, steadyPackageRate);
            udpEndpoint.OnMessageReceived += (x, y) => RawOnMessageReceived?.Invoke(x, y);
            udpEndpoint.OnPacketFailedToSend += (x) => RawOnPacketFailedToSend?.Invoke(x);
            udpEndpoint.OnMessageReceived += UdpEndpoint_OnMessageReceived;
        }

        public void OpenSession(IPEndPoint endpoint)
        {
            var packet = new OpenSessionRequestPacket();
            SendImmediateReliable(packet, endpoint);
        }

        public void RegisterService(INetworkService service)
        {
            service.packetQueueEvent += udpEndpoint.BufferMessage;
            service.immediatePacketSendRequestEvent += Service_immediatePacketSendRequestEvent;
            udpEndpoint.OnPacketFailedToSend += service.OnPacketFailedToSend;
            udpEndpoint.OnMessageReceived += service.OnMessageReceived;
            services.Add(service);

            service.InjectSessions(sessions);
        }

        public void QueuePacket(INetworkPacket packet, IPEndPoint endPoint)
        {
            udpEndpoint.BufferMessage(endPoint, packet);
        }

        public void SendImmediateReliable(INetworkPacket packet, IPEndPoint endpoint)
        {
            _ = udpEndpoint.SendMessageAsync(endpoint, packet, true);
        }

        public void SendImmediate(INetworkPacket packet, IPEndPoint endpoint)
        {
            _ = udpEndpoint.SendMessageAsync(endpoint, packet, false);
        }

        public T GetService<T>() where T : INetworkService
        {
            return services.OfType<T>().FirstOrDefault();
        }

        private void UdpEndpoint_OnMessageReceived(NetworkPacket packet, IPEndPoint endPoint)
        {
            if (packet.packetType == OpenSessionRequestPacket.PacketType)
            {
                OpenSessionRequestPacket openSessionPacket = new OpenSessionRequestPacket();
                openSessionPacket.Deserialize(packet.payload, 0, packet.payload.Length);

                var session = new UDPSession(openSessionPacket, endPoint);
                sessions.Add(session);
            }
        }

        private void Service_immediatePacketSendRequestEvent(IPEndPoint endPoint, INetworkPacket packet, bool reliable)
        {
            _ = udpEndpoint.SendMessageAsync(endPoint, packet, reliable);
        }
    }
}
