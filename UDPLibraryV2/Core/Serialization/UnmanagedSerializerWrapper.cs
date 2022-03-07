using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Serialization
{
    internal struct UnmanagedSerializerWrapper<T> : INetworkSerializable where T : unmanaged
    {
        public short TypeId { get; set; }

        public short MinimumBufferSize { get { unsafe { return (short)sizeof(T); } } }

        public T Value { get; set; }

        public UnmanagedSerializerWrapper(T value, short typeId)
        {
            TypeId = typeId;
            Value = value;
        }

        public void Deserialize(byte[] buffer, int start)
        {
            Value = ValueSerializer.NetworkValueDeSerialize<T>(buffer, start);
        }

        public void Serialize(byte[] buffer, int start)
        {
            ValueSerializer.NetworkValueSerialize(Value, buffer, start);
        }
    }
}
