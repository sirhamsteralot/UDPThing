using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public class PacketFactory
    {
        public readonly static uint networkingVersion = 1;

        public static unsafe NetworkPacket CreatePacket(INetworkPacket packet, uint index, bool reliable)
        {
            byte[] payload = new byte[packet.GetSize() + 13];

            uint packetType = packet.GetType();

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
                packetIndex = index,
                packetType = packetType,
                reliablePacket = reliable,
            };

            networkPacket.payload = payload;

            return networkPacket;
        }
    }
}
