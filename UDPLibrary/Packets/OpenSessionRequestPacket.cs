using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class OpenSessionRequestPacket : INetworkPacket
    {
        public const int PacketType = 4;
        public uint sessionVersion = 1;

        public OpenSessionRequestPacket()
        {

        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                *(uint*)ptr = sessionVersion;
            }
        }

        public unsafe void Deserialize(byte[] payload, int start, int length)
        {
            fixed (byte* ptr = &payload[start])
            {
                sessionVersion = *(uint*)ptr;
            }
        }

        public int GetSize()
        {
            return 4;
        }

        public uint GetPacketType()
        {
            return PacketType;
        }
    }
}
