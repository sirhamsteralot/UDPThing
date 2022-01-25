using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary.Packets;
using System.Security.Cryptography;
using UDPLibrary.Services;
using UDPLibrary.Core;

namespace UDPLibrary
{
    public class UDPInterface
    {
        public ConcurrentDictionary<IPEndPoint, UDPSession> sessions;

        public ConcurrentDictionary<IPEndPoint, DateTime> sessionOpenAttempt;

        public List<INetworkService> services;
        public UDPCore udpEndpoint;

        public event Action<NetworkPacket, IPEndPoint>? RawOnPacketReceived;
        public event Action<NetworkPacket>? RawOnPacketFailedToSend;

        public event Action<UDPSession> OnSessionTimedOut;

        private int _timeout;

        public UDPInterface(int listenPort = 0, int steadyPackageRate = 60, int maxRetries = 3, int timeout = 2500)
        {
            _timeout = timeout;

            sessions = new ConcurrentDictionary<IPEndPoint, UDPSession>();
            services = new List<INetworkService>();
            sessionOpenAttempt = new ConcurrentDictionary<IPEndPoint, DateTime>();

            udpEndpoint = new UDPCore(listenPort, maxRetries, timeout, steadyPackageRate);
            udpEndpoint.OnPacketReceived += (x, y) => RawOnPacketReceived?.Invoke(x, y);
            udpEndpoint.OnPacketFailedToSend += (x) => RawOnPacketFailedToSend?.Invoke(x);
            udpEndpoint.OnPacketReceived += UdpEndpoint_OnPacketReceived;
        }


        public async Task<UDPSession> OpenSession(IPEndPoint endpoint)
        {
            var packet = new OpenSessionRequestPacket();
            if (!sessionOpenAttempt.TryAdd(endpoint, DateTime.Now))
                return null;

            await udpEndpoint.SendPacketAsync(endpoint, packet, true);

            for (int i = 0; i < _timeout; i += _timeout / 10)
            {
                await Task.Delay(_timeout / 100);

                if (sessions.TryGetValue(endpoint, out var session))
                {
                    return session;
                }
            }

            return null;
        }

        public void RegisterService(INetworkService service)
        {
            service.packetQueueEvent += udpEndpoint.BufferPacket;
            service.immediatePacketSendRequestEvent += Service_immediatePacketSendRequestEvent;
            udpEndpoint.OnPacketReceived += service.OnPacketReceived;
            services.Add(service);

            service.InjectSessions(sessions);
        }

        public void QueuePacket(INetworkPacket packet, IPEndPoint endPoint)
        {
            udpEndpoint.BufferPacket(endPoint, packet);
        }

        public async Task SendImmediateReliable(INetworkPacket packet, IPEndPoint endpoint)
        {
            await udpEndpoint.SendPacketAsync(endpoint, packet, true);
        }

        public async Task SendImmediate(INetworkPacket packet, IPEndPoint endpoint)
        {
            await udpEndpoint.SendPacketAsync(endpoint, packet, false);
        }

        public T GetService<T>() where T : INetworkService
        {
            return services.OfType<T>().FirstOrDefault();
        }

        private void UdpEndpoint_OnPacketReceived(NetworkPacket packet, IPEndPoint endPoint)
        {
            switch (packet.packetType)
            {
                case OpenSessionRequestPacket.PacketType:
                    AcceptSessionRequest(packet, endPoint);
                    break;
                case SessionAcceptedPacket.PacketType:
                    SessionAccepted(packet, endPoint);
                    break;
            }

            if (sessions.TryGetValue(endPoint, out UDPSession outSession))
                outSession.OnPacketReceived(packet, endPoint);
        }

        private async void AcceptSessionRequest(NetworkPacket packet, IPEndPoint endPoint)
        {
            OpenSessionRequestPacket openSessionPacket = new OpenSessionRequestPacket();
            openSessionPacket.Deserialize(packet.payload, 0, packet.payload.Length);

            var session = new UDPSession(openSessionPacket, endPoint, udpEndpoint);
            session.SetTimeout(_timeout, OnSessionTimedOutCallBack);
            bool success = sessions.TryAdd(endPoint, session);

            var acceptancePacket = new SessionAcceptedPacket(success);

            await udpEndpoint.SendPacketAsync(endPoint, acceptancePacket, true);
        }

        private void SessionAccepted(NetworkPacket packet, IPEndPoint endPoint)
        {
            if (sessionOpenAttempt.TryGetValue(endPoint, out var timestamp))
            {
                if ((DateTime.Now - timestamp).TotalMilliseconds > _timeout)
                    return;
            }

            SessionAcceptedPacket acceptedPacket = new SessionAcceptedPacket();
            acceptedPacket.Deserialize(packet.payload, 0, packet.payload.Length);

            if (acceptedPacket.SessionAccepted)
            {
                var session = new UDPSession(acceptedPacket, endPoint, udpEndpoint);
                session.SetTimeout(_timeout, OnSessionTimedOutCallBack);
                sessions.TryAdd(endPoint, session);
            }
        }

        private void OnSessionTimedOutCallBack(object? state)
        {
            UDPSession session = (UDPSession)state;

            OnSessionTimedOut?.Invoke(session);

            sessions.Remove(session.remoteEP, out var sessionRef);
            sessionRef.Dispose();
        }

        private void Service_immediatePacketSendRequestEvent(IPEndPoint endPoint, INetworkPacket packet, bool reliable)
        {
            _ = udpEndpoint.SendPacketAsync(endPoint, packet, reliable);
        }
    }
}
