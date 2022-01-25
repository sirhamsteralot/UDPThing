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
        public IPEndPoint remoteEP;
        public uint sessionVersion;

        private int _timeoutMS;
        private Timer _timeoutTimer;

        private UDPCore _udpCore;

        public UDPSession(OpenSessionRequestPacket openSessionRequest, IPEndPoint source, UDPCore udpCore) : this(udpCore ,source)
        {
            sessionVersion = openSessionRequest.sessionVersion;
        }

        public UDPSession(SessionAcceptedPacket sessionAcceptedPacket, IPEndPoint source, UDPCore udpCore) : this(udpCore, source)
        {
            sessionVersion = sessionAcceptedPacket.sessionVersion;
        }

        private UDPSession(UDPCore udpCore, IPEndPoint source)
        {
            remoteEP = source;
            _udpCore = udpCore;
        }

        public void SetTimeout(int timeoutMS, TimerCallback timeoutCallback)
        {
            _timeoutMS = timeoutMS;
            _timeoutTimer = new Timer(timeoutCallback, this, timeoutMS, Timeout.Infinite);
        }

        public void BufferPacket(INetworkPacket packet)
        {
            _udpCore.BufferPacket(remoteEP, packet);
        }

        public void OnPacketReceived(NetworkPacket packet, IPEndPoint endPoint)
        {
            _timeoutTimer?.Change(_timeoutMS, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timeoutTimer.Dispose();
            _timeoutTimer = null;
        }
    }
}
