using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class PacketHelper
    {
        public readonly static uint networkingVersion = 1;

        public static NetworkPacket CreatePacket(INetworkPacket packet, uint index, bool reliable)
        {
            return CreatePacket(packet, index, Array.Empty<byte>(), reliable);
        }

        public static unsafe NetworkPacket CreatePacket(INetworkPacket packet, uint index, byte[] headers, bool reliable)
        {
            int headersSize = 17 + headers.Length;

            byte[] payload = new byte[packet.GetSize() + headersSize];

            uint packetType = packet.GetPacketType();

            fixed (byte* pbytes = &payload[0])
            {
                *(uint*)pbytes = networkingVersion;
                *(uint*)(pbytes + 4) = packetType;
                *(uint*)(pbytes + 8) = index;
                *(bool*)(pbytes + 12) = reliable;
                *(int*)(pbytes + 13) = headers.Length;
            }

            Buffer.BlockCopy(headers, 0, payload, headersSize, headers.Length);

            packet.Serialize(payload, headersSize);

            var networkPacket = new NetworkPacket()
            {
                packetVersion = networkingVersion,
                packetId = index,
                packetType = packetType,
                reliablePacket = reliable,
                headerLength = headers.Length,
                headers = headers
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
                    return typeof(PermissionsRequestPacket);

                default:
                    return null;
            }
        }
    }
}
