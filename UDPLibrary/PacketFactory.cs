using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UDPLibrary.Packets;

namespace UDPLibrary
{
    public class PacketFactory
    {
        public readonly static uint networkingVersion = 1;

        public static unsafe NetworkPacket CreatePacket(INetworkPacket packet, uint index, bool reliable)
        {
            byte[] payload = new byte[packet.GetSize() + 13];

            uint packetType = packet.GetPacketType();

            fixed (byte* pbytes = &payload[0])
            {
                *(uint*)pbytes = networkingVersion;
                *(uint*)(pbytes + 4) = packetType;
                *(uint*)(pbytes + 8) = index;
                *(bool*)(pbytes + 12) = reliable;
            }

            packet.Serialize(payload, 13);

            var networkPacket = new NetworkPacket()
            {
                packetVersion = networkingVersion,
                packetId = index,
                packetType = packetType,
                reliablePacket = reliable,
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

                default:
                    return null;
            }
        }
    }
}
