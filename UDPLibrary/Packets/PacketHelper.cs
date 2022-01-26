using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class PacketHelper
    {
        public readonly static uint networkingVersion = 1;

        public static Dictionary<uint, Type> packetTypeCache;

        public static NetworkPacket CreatePacket(INetworkPacket packet, uint index, bool reliable)
        {
            return CreatePacket(packet, index, reliable, 0, 0, 0);
        }

        public static NetworkPacket CreatePacket(INetworkPacket packet, uint index, bool reliable, int eventstreamid)
        {
            return CreatePacket(packet, index, reliable, eventstreamid, 0, 0);
        }

        public static NetworkPacket CreatePacket(INetworkPacket packet, uint index, bool reliable, uint sessionId, byte sessionSeq)
        {
            return CreatePacket(packet, index, reliable, 0, sessionId, sessionSeq);
        }

        public static unsafe NetworkPacket CreatePacket(INetworkPacket packet, uint index, bool reliable, int eventstreamId, uint sessionId, byte sessionSeq)
        {
            byte[] payload = new byte[packet.GetSize() + NetworkPacket.PACKET_SIZE];

            uint packetType = packet.GetPacketType();

            fixed (byte* pbytes = &payload[0])
            {
                *(uint*)pbytes = networkingVersion;
                *(uint*)(pbytes + 4) = packetType;
                *(uint*)(pbytes + 8) = index;
                *(bool*)(pbytes + 12) = reliable;
                *(int*)(pbytes + 13) = eventstreamId;
                *(uint*)(pbytes + 17) = sessionId;
                *(pbytes + 21) = sessionSeq;
            }

            packet.Serialize(payload, NetworkPacket.PACKET_SIZE);

            var networkPacket = new NetworkPacket()
            {
                packetVersion = networkingVersion,
                packetId = index,
                packetType = packetType,
                reliablePacket = reliable,
                eventstreamId = eventstreamId,
                sessionId = sessionId,
                sessionSequence = sessionSeq
            };

            networkPacket.payload = payload;

            return networkPacket;
        }

        public static NetworkPacket CreateAckPacket(uint packetToAck, uint broadCastCount)
        {
            return CreatePacket(new AckPacket(packetToAck), broadCastCount, false);
        }

        public static Type? GetPacketType(uint packetType)
        {
            switch (packetType)
            {
                case 1:
                    return typeof(AckPacket);
                case 2:
                    return typeof(CompositePacket);
                case 3:
                    return typeof(TestPacket);
                case 4:
                    return typeof(OpenSessionRequestPacket);
                case 5:
                    return typeof(SessionAcceptedPacket);
                case 6:
                    return typeof(KeepAlivePacket);

                default:
                    return null;
            }
        }
    }
}
