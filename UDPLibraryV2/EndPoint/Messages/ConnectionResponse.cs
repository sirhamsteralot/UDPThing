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

        public short RequiredSendBufferSize => (short)(5 + (AvailableProcedures.Length * 2));

        public Permissions GrantedPermissions;
        public bool ConnectionGranted;
        public short ProcedureCount;
        public short[] AvailableProcedures;

        public unsafe void Deserialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                GrantedPermissions = *(Permissions*)(ptr);
                ConnectionGranted = *(bool*)(ptr + 4);
                ProcedureCount = *(short*)(ptr + 5);
            }

            AvailableProcedures = new short[ProcedureCount];
            Buffer.BlockCopy(buffer, start + 5, AvailableProcedures, 0, ProcedureCount / 2);
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                *(Permissions*)(ptr) = GrantedPermissions;
                *(bool*)(ptr + 4) = ConnectionGranted;
                *(short*)(ptr + 5) = (short)AvailableProcedures.Length;
            }

            Buffer.BlockCopy(AvailableProcedures, 0, buffer, 5, AvailableProcedures.Length * 2);
        }
    }
}
