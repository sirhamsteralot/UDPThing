using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Serialization
{
    internal class TypeProvider
    {
        public static TypeProvider Instance = new TypeProvider();

        public ConcurrentDictionary<short, Type> typeMap = new ConcurrentDictionary<short, Type>();

        public TypeProvider()
        {

        }

        public Type GetType(short typeId)
        {
            return typeMap[typeId];
        }

        public bool TryRegisterType(short typeId, Type type)
        {
            return typeMap.TryAdd(typeId, type);
        }
    }
}
