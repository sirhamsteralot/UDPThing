using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public class NetworkPacket
    {
        public int packetVersion;
        public int packetType;
        public int packetIndex;
        public bool reliablePacket;

        public byte[] payload;

        public NetworkPacket(byte[] incoming)
        {
            packetVersion = (incoming[0] << (3 * 8)) | (incoming[1] << (2 * 8)) | (incoming[2] << 8) | incoming[3];
            packetType = (incoming[4] << (3 * 8)) | (incoming[5] << (2 * 8)) | (incoming[6] << 8) | incoming[7];
            packetIndex = (incoming[8] << (3 * 8)) | (incoming[9] << (2 * 8)) | (incoming[10] << 8) | incoming[11];
            reliablePacket = incoming[12] == 0;

            payload = incoming;
        }

        public NetworkPacket(int packetVersion, int packetType, int packetIndex, bool reliablePacket)
        {
            this.packetVersion = packetVersion;
            this.packetType = packetType;
            this.packetIndex = packetIndex;
            reliablePacket = reliablePacket;
        }

        public void Serialize<T>(T input, int size) where T : ISerializable
        {
            payload = new byte[size + 13];

            BitConverter.GetBytes(packetVersion).CopyTo(payload, 0);
            BitConverter.GetBytes(packetType).CopyTo(payload, 4);
            BitConverter.GetBytes(packetIndex).CopyTo(payload, 8);
            BitConverter.GetBytes(reliablePacket).CopyTo(payload, 12);

            input.Serialize(payload, 13);
        }

        public void Deserialize<T>(ref T output) where T : ISerializable
        {
            output.Deserialize(payload, 13, payload.Length - 13);
        }
    }
}
