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
        public uint sessionVersion = 2;

        public uint sessionId;
        public bool sessionAccepted;
        
        public SessionAcceptedPacket()
        {

        }

        public SessionAcceptedPacket(bool sessionAccepted, uint sessionId)
        {
            this.sessionAccepted = sessionAccepted;
            this.sessionId = sessionId;
        }

        public unsafe void Deserialize(byte[] payload, int start, int length)
        {
            sessionAccepted |= payload[start] != 0;

            fixed (byte* p = &payload[start + 1])
            {
                sessionVersion = *(uint*)p;
                sessionId = *(uint*)(p+4);
            }
        }

        public uint GetPacketType()
        {
            return PacketType;
        }

        public int GetSize()
        {
            return 9;
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            buffer[start] = sessionAccepted ? (byte)1 : (byte)0;
            fixed (byte* p = &buffer[start + 1])
            {
                *(uint*)p = sessionVersion;
                *(uint*)(p + 4) = sessionId;
            }
        }
    }
}
