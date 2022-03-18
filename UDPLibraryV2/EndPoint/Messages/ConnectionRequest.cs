using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.RPC;

namespace UDPLibraryV2.EndPoint.Messages
{
    internal class ConnectionRequest : IRequest
    {
        public short ResponseTypeId => 2;
        public short TypeId => 1;
        public short RequiredSendBufferSize => (short)(8 + (AvailableProcedures.Length * 2));


        public short ConnectionVersion;
        public Permissions RequestedPermissions;
        public short ProcedureCount;
        public short[] AvailableProcedures;

        public unsafe void Deserialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                ConnectionVersion = *(short*)ptr;
                RequestedPermissions = *(Permissions*)(ptr + 2);
                ProcedureCount = *(short*)(ptr + 6);
            }

            AvailableProcedures = new short[ProcedureCount];
            Buffer.BlockCopy(buffer, start + 8, AvailableProcedures, 0, ProcedureCount / 2);
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                *(short*)ptr = ConnectionVersion;
                *(Permissions*)(ptr + 2) = RequestedPermissions;
                *(short*)(ptr + 6) = (short)AvailableProcedures.Length;
            }

            Buffer.BlockCopy(AvailableProcedures, 0, buffer, 8, AvailableProcedures.Length * 2);
        }
    }
}
