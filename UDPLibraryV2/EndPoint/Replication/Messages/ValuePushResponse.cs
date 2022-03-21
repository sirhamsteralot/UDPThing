using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.RPC;

namespace UDPLibraryV2.EndPoint.Replication.Messages
{
    internal struct ValuePushResponse : IResponse
    {
        public bool DoCompress => false;

        public short TypeId => 6;

        public short RequiredSendBufferSize => 1;

        public byte successFlags;

        public void Deserialize(byte[] buffer, int start)
        {
            successFlags = buffer[start];
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            buffer[start] = successFlags;
        }
    }
}
