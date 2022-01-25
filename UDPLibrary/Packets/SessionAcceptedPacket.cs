using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class SessionAcceptedPacket : INetworkPacket
    {
        public const int PacketType = 5;
        public uint sessionVersion = 1;
        public bool SessionAccepted;
        
        public SessionAcceptedPacket()
        {

        }

        public SessionAcceptedPacket(bool sessionAccepted)
        {
            SessionAccepted = sessionAccepted;
        }

        public unsafe void Deserialize(byte[] payload, int start, int length)
        {
            SessionAccepted |= payload[start] != 0;

            fixed (byte* p = &payload[start + 1])
            {
                sessionVersion = *(uint*)p;
            }
        }

        public uint GetPacketType()
        {
            return PacketType;
        }

        public int GetSize()
        {
            return 5;
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            buffer[start] = SessionAccepted ? (byte)1 : (byte)0;
            fixed (byte* p = &buffer[start + 1])
            {
                *(uint*)p = sessionVersion;
            }
        }
    }
}
