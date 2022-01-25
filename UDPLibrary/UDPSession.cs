using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary.Packets;
using System.Security.Cryptography;

namespace UDPLibrary
{
    public class UDPSession
    {
        public IPEndPoint endpoint;
        public uint sessionVersion;

        public uint permissions;

        public DateTime lastHeardFrom;

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
            endpoint = source;
            _udpCore = udpCore;
        }

        public void BufferPacket(INetworkPacket packet)
        {
            _udpCore.BufferPacket(endpoint, packet);
        }

        public async Task<bool> RequestPermissions(uint permissions)
        {
            _udpCore.OnPacketReceived += _udpCore_OnPacketReceived;

            PermissionsRequestPacket requestPacket = new PermissionsRequestPacket(permissions, RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue));

            NetworkPacket responsePacket = await _udpCore.RequestResponseAsync(endpoint, requestPacket, true);
            requestPacket.Deserialize(responsePacket.payload, 0, responsePacket.payload.Length);


        }

        private void _udpCore_OnPacketReceived(NetworkPacket packet, IPEndPoint ep)
        {
            if (ep != endpoint)
                return;
        }
    }
}
