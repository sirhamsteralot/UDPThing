using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.RPC;

namespace UDPLibraryV2.EndPoint.Messages
{
    internal class ConnectionResponse : IResponse
    {
        public bool DoCompress => false;

        public short TypeId => 2;

        public short RequiredSendBufferSize => 5;

        public Permissions GrantedPermissions;
        public bool ConnectionGranted;

        public unsafe void Deserialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                GrantedPermissions = *(Permissions*)(ptr);
                ConnectionGranted = *(bool*)(ptr + 4);
            }
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                *(Permissions*)(ptr) = GrantedPermissions;
                *(bool*)(ptr + 4) = ConnectionGranted;
            }
        }
    }
}
