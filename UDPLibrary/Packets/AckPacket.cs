using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class AckPacket : INetworkPacket
    {
        public const uint packetType = 1;
        private bool ack;
        private uint ackPackage;

        public AckPacket(uint ackPackage)
        {
            this.ackPackage = ackPackage;
            ack = true;
        }

        public unsafe void Deserialize(byte[] payload, int start, int length)
        {
            fixed (byte* pbytes = &payload[start])
            {
                ack = *(bool*)pbytes;
                ackPackage = *(uint*)(pbytes + 1);
            }
        }

        public int GetSize()
        {
            return 5;
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* pbytes = &buffer[start])
            {
                *(bool*)pbytes = ack;
                *(uint*)(pbytes + 1) = ackPackage;
            }
        }

        uint INetworkPacket.GetType()
        {
            return packetType;
        }
    }
}
