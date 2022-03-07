using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Serialization;

namespace UDPServerV2
{
    internal class RandomSerializable : INetworkSerializable
    {
        private readonly static short _typeId = 3123;

        public short TypeId => _typeId;

        public short MinimumBufferSize { get; set; }

        public byte[] bytes;

        public RandomSerializable(short bufferSize)
        {
            MinimumBufferSize = bufferSize;

            bytes = new byte[bufferSize];

            RandomNumberGenerator.Fill(new Span<byte>(bytes).Slice(0, bytes.Length - bytes.Length / 8));
        }

        public void Deserialize(byte[] buffer, int start)
        {
            bytes = buffer[start..MinimumBufferSize];
        }

        public void Serialize(byte[] buffer, int start)
        {
            Array.Copy(bytes, 0, buffer, start, bytes.Length);
        }
    }
}
