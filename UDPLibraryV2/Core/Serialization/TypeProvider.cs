using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Serialization
{
    internal class TypeProvider
    {
        public static TypeProvider Instance = new TypeProvider();

        public Dictionary<short, Type> typeMap = new Dictionary<short, Type>();

        public TypeProvider()
        {

        }

        public Type GetType(short typeId)
        {
            return typeMap[typeId];
        }

        public void RegisterType(short typeId, Type type)
        {
            typeMap.Add(typeId, type);
        }
    }
}
