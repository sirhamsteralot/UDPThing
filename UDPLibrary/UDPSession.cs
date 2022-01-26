using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary.Packets;
using System.Security.Cryptography;
using UDPLibrary.Core;
using System.Threading;

namespace UDPLibrary
{
    public class UDPSession : IDisposable
    {
        public event Action<NetworkPacket, UDPSession> OnPacketReceived;
        public event Action<List<INetworkPacket>, UDPSession> OnCompositePacketReceived;
        public event Action<UDPSession> OnSessionTimeout;

        public IPEndPoint remoteEP;
        public uint sessionVersion;

        public uint sessionId;
        public byte sessionSeq;
        public byte lastSeqReceived = (byte)254;

        private long _totalReceived;

        private int _timeoutMS;
        private Timer _timeoutTimer;

        private int _keepaliveMs;
        private Timer _keepAliveTimer;

        private UDPCore _udpCore;

        public UDPSession(OpenSessionRequestPacket openSessionRequest, IPEndPoint source, UDPCore udpCore, uint sessionId) : this(udpCore, source)
        {
            sessionVersion = openSessionRequest.sessionVersion;
            this.sessionId = sessionId;
        }

        public UDPSession(SessionAcceptedPacket sessionAcceptedPacket, IPEndPoint source, UDPCore udpCore) : this(udpCore, source)
        {
            sessionVersion = sessionAcceptedPacket.sessionVersion;
            sessionId = sessionAcceptedPacket.sessionId;
        }

        private UDPSession(UDPCore udpCore, IPEndPoint source)
        {
            remoteEP = source;
            _udpCore = udpCore;
        }

        public void SetTimeout(int timeoutMS)
        {
            _timeoutMS = timeoutMS;
            _timeoutTimer = new Timer(x => 
            {
                OnSessionTimeout?.Invoke(this);
            }, this, timeoutMS, Timeout.Infinite);
        }

        public void SetKeepAlive(int keepAliveLoopMs)
        {
            _keepaliveMs = keepAliveLoopMs;
            _keepAliveTimer = new Timer(OnKeepAliveTrigger, null, 0, _keepaliveMs);
        }

        public void BufferPacket(INetworkPacket packet)
        {
            _udpCore.BufferPacket(remoteEP, packet, sessionId, sessionSeq++);

            ExtendKeepAliveTimer();
        }

        internal void OnPacketReceivedCallBack(NetworkPacket packet, IPEndPoint endPoint)
        {
            if (packet.sessionId != sessionId)
                return;

            int diff = packet.sessionSequence - lastSeqReceived;

            if (diff < -(byte.MaxValue / 2))
                diff += byte.MaxValue;
            else if (diff > (byte.MaxValue / 2))
                diff -= byte.MaxValue;

            if (diff < 1)
                return;

            lastSeqReceived = packet.sessionSequence;
            _totalReceived++;
            ExtendTimeout();

            if (packet.packetType == CompositePacket.PacketType)
            {
                CompositePacket unpackedComposite = new CompositePacket();
                packet.Deserialize(ref unpackedComposite);

                OnCompositePacketReceived?.Invoke(unpackedComposite.subPackets, this);
            }

            OnPacketReceived?.Invoke(packet, this);
        }

        private void OnKeepAliveTrigger(object? state)
        {
            KeepAlivePacket packet = new KeepAlivePacket();
            _udpCore.BufferPacket(remoteEP, packet, sessionId, sessionSeq++);

            ExtendKeepAliveTimer();
        }

        private void ExtendTimeout()
        {
            _timeoutTimer?.Change(_timeoutMS, Timeout.Infinite);

            if (UDPInterface.SELFLOGGING)
                Console.WriteLine($"Timeout for session {sessionId} extended!");
        }

        private void ExtendKeepAliveTimer()
        {
            _keepAliveTimer.Change(_keepaliveMs, _keepaliveMs);

            if (UDPInterface.SELFLOGGING)
                Console.WriteLine($"Keepalive for session {sessionId} extended!");
        }

        public void Dispose()
        {
            _timeoutTimer?.Dispose();
            _timeoutTimer = null;

            _keepAliveTimer?.Dispose();
            _keepAliveTimer = null;
        }
    }
}
