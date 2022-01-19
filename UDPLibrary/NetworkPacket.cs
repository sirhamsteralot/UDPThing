using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public class NetworkPacket
    {
        public uint packetVersion;
        public uint packetType;
        public uint packetIndex;
        public bool reliablePacket;

        public byte[] payload;

        public NetworkPacket()
        {

        }

        public unsafe NetworkPacket(byte[] incoming)
        {
            fixed (byte* pbytes = &incoming[0])
            {
                packetVersion = *(uint*)pbytes;
                packetType = *(uint*)(pbytes + 4);
                packetIndex = *(uint*)(pbytes + 8);
                reliablePacket = *(bool*)(pbytes + 12);
            }

            payload = incoming;
        }

        public void Deserialize<T>(ref T output) where T : INetworkPacket
        {
            output.Deserialize(payload, 13, payload.Length - 13);
        }
    }
}
