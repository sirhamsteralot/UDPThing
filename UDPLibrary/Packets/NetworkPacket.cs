using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class NetworkPacket
    {
        public const int PACKET_SIZE = 22;

        public uint packetVersion;
        public uint packetType;
        public uint packetId;
        public bool reliablePacket;
        public int eventstreamId;
        public uint sessionId;
        public byte sessionSequence;

        public byte[] payload;

        internal NetworkPacket()
        {

        }

        public unsafe NetworkPacket(byte[] incoming)
        {
            fixed (byte* pbytes = &incoming[0])
            {
                packetVersion = *(uint*)pbytes;
                packetType = *(uint*)(pbytes + 4);
                packetId = *(uint*)(pbytes + 8);
                reliablePacket = *(bool*)(pbytes + 12);
                eventstreamId = *(int*)(pbytes + 13);
                sessionId = *(uint*)(pbytes + 17);
                sessionSequence = *(pbytes + 21);
            }

            payload = incoming;
        }

        public void Deserialize<T>(ref T output) where T : INetworkPacket
        {
            output.Deserialize(payload, PACKET_SIZE, payload.Length - PACKET_SIZE);
        }
    }
}
