using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class PermissionsRequestPacket : INetworkPacket
    {
        public const int PacketType = 6;

        public uint RequestedPermissions;
        public int Nonce;
        public bool PermissionsGranted;

        public PermissionsRequestPacket() { }

        public PermissionsRequestPacket(uint requestedPermissions, int nonce)
        {
            RequestedPermissions = requestedPermissions;
            Nonce = nonce;
        }

        public unsafe void Deserialize(byte[] payload, int start, int length)
        {
            fixed (byte* p = &payload[start])
            {
                RequestedPermissions = *(uint*)p;
                Nonce = *(int*)(p + 4);
                PermissionsGranted = *(bool*)(p + 8);
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
            fixed (byte* p = &buffer[start])
            {
                *(uint*)p = RequestedPermissions;
                *(int*)(p + 4) = Nonce;
                *(bool*)(p + 8) = PermissionsGranted;
            }
        }
    }
}
