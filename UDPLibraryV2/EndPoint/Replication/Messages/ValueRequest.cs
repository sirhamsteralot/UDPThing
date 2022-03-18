using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.RPC;

namespace UDPLibraryV2.EndPoint.Replication.Messages
{
    internal class ValueRequest : IRequest
    {
        public short requestedInstanceId;
        public short requestedTypeId;

        public ValueRequest()
        {

        }

        public ValueRequest(short requestedTypeId, short instanceId)
        {
            this.requestedInstanceId = instanceId;
            this.requestedTypeId = requestedTypeId;
        }

        public short ResponseTypeId => 4;

        public short TypeId => 3;

        public short RequiredSendBufferSize => 4;

        public unsafe void Deserialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                requestedInstanceId = *(short*)ptr;
                requestedTypeId = *(short*)(ptr + 2);
            }
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                *(short*)ptr = requestedInstanceId;
                *(short*)(ptr + 2) = requestedTypeId;
            }
        }
    }
}
