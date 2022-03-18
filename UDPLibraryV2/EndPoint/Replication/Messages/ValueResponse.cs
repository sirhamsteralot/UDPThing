using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Serialization;
using UDPLibraryV2.RPC;

namespace UDPLibraryV2.EndPoint.Replication.Messages
{
    internal class ValueResponse : IResponse
    {
        public bool DoCompress => false;

        public short TypeId => 4;

        public short RequiredSendBufferSize => (short)(valueSize + 4);

        public int valueSize;
        public byte[] value;

        public static ValueResponse CreateFailedResponse()
        {
            return new ValueResponse(null)
            {
                valueSize = 0,
            };
        }

        public ValueResponse()
        {
            valueSize = 0;
        }

        public ValueResponse(INetworkSerializable serializable)
        {
            if (serializable == null)
                return;

            valueSize = serializable.RequiredSendBufferSize;
            value = new byte[valueSize];
            serializable.Serialize(value, 0);
        }

        public unsafe void Deserialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                valueSize = *(int*)ptr;
            }

            if (valueSize > 0)
            {
                value = new byte[valueSize];
                Buffer.BlockCopy(buffer, start + 4, value, 0, valueSize);
            }
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                *(int*)ptr = valueSize;
            }

            if (valueSize > 0)
                Buffer.BlockCopy(value, 0, buffer, start + 4, value.Length);
        }
    }
}
