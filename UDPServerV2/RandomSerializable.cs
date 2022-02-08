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
        public short TypeId => 1;

        public short MinimumBufferSize { get; set; }

        public byte[] bytes;

        public RandomSerializable(short bufferSize)
        {
            MinimumBufferSize = bufferSize;

            bytes = new byte[bufferSize];

            RandomNumberGenerator.Fill(bytes);
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
