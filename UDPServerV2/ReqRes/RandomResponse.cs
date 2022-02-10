using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.RequestResponse;

namespace UDPServerV2.ReqRes
{
    internal class RandomResponse : IResponse
    {
        public short TypeId => 3;

        public short MinimumBufferSize { get; set; }

        public byte[] bytes;

        public RandomResponse(short bufferSize)
        {
            MinimumBufferSize = bufferSize;

            bytes = new byte[bufferSize];

            RandomNumberGenerator.Fill(bytes);
        }

        public RandomResponse()
        {

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
