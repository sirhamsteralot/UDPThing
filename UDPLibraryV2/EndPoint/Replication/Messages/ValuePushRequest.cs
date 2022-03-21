using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.RPC;

namespace UDPLibraryV2.EndPoint.Replication.Messages
{
    internal class ValuePushRequest : IRequest
    {
        public short ResponseTypeId => 6;
        public short TypeId => 5;
        public short RequiredSendBufferSize => (short)(6 + value.Length);

        public short valueInstanceId;
        public short valueTypeId;
        public short valueSize;

        public byte[] value;

        public ValuePushRequest(short valueInstanceId, short valueTypeId, byte[] value)
        {
            this.valueInstanceId = valueInstanceId;
            this.valueTypeId = valueTypeId;
            this.valueSize = (short)value.Length;
            this.value = value;
        }

        public ValuePushRequest()
        {

        }

        public unsafe void Deserialize(byte[] buffer, int start)
        {
            fixed (byte* valuePtr = &buffer[start])
            {
                valueInstanceId = *(short*)valuePtr;
                valueTypeId = *(short*)(valuePtr + 2);
                valueSize = *(short*)(valuePtr + 4);
            }

            value = new byte[valueSize];
            Buffer.BlockCopy(buffer, start + 6, value, 0, valueSize);
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* valuePtr = &buffer[start])
            {
                *(short*)valuePtr = valueInstanceId;
                *(short*)(valuePtr + 2) = valueTypeId;
                *(short*)(valuePtr + 4) = valueSize;
            }

            Buffer.BlockCopy(value, 0, buffer, start + 6, valueSize);
        }
    }
}
