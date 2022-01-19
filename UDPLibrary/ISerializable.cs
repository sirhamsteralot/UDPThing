using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public interface ISerializable
    {
        public void Serialize(byte[] buffer, int start);
        public void Deserialize(byte[] payload, int start, int length);
        public int GetSize();
    }
}
