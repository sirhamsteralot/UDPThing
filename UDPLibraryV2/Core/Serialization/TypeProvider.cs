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

        private static Dictionary<short, Type> types = new Dictionary<short, Type>();

        public static short CreateTypeId(Type type)
        {
            var hashed = md5HashingProvider.ComputeHash(Encoding.UTF8.GetBytes(type.FullName));

            short typeId = (short)((hashed[0] << 8) | hashed[1]);
            types.Add(typeId, type);

            return typeId;
        }

        public static Type GetTypeFromId(short id)
        {
            return types[id];
        }
    }
}
