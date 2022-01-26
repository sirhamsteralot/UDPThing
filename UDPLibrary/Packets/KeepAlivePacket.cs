using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class KeepAlivePacket : INetworkPacket
    {
        public const uint PacketType = 6;

        public void Deserialize(byte[] payload, int start, int length)
        {

        }

        public uint GetPacketType()
        {
            return PacketType;
        }

        public int GetSize()
        {
            return 0;
        }

        public void Serialize(byte[] buffer, int start)
        {

        }
    }
}
