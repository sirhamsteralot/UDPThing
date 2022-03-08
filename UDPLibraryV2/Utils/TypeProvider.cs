using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Serialization
{
    public class TypeProvider
    {
        private static MD5 md5HashingProvider = MD5.Create();

        public static short GetShortTypeId(Type type)
        {
            var hashed = md5HashingProvider.ComputeHash(Encoding.UTF8.GetBytes(type.FullName));

            short typeId = (short)((hashed[0] << 8) | hashed[1]);

            return typeId;
        }

        public static Span<byte> GetTypeHash(Type type)
        {
            return md5HashingProvider.ComputeHash(Encoding.UTF8.GetBytes(type.FullName));
        }
    }
}
