using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class NetworkPacket
    {
        public uint packetVersion;
        public uint packetType;
        public uint packetId;
        public bool reliablePacket;
        public int eventstreamId;

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
            }

            payload = incoming;
        }

        public void Deserialize<T>(ref T output) where T : INetworkPacket
        {
            int totalSize = 17;
            output.Deserialize(payload, totalSize, payload.Length - totalSize);
        }
    }
}
