using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Events
{
    public class TestPacket : ISerializable
    {
        public string thisisavalue = "";

        public void Deserialize(byte[] payload, int start, int length)
        {
            thisisavalue = Encoding.ASCII.GetString(payload, start, length);
        }

        public int GetSize()
        {
            return thisisavalue.Length;
        }

        public void Serialize(byte[] buffer, int start)
        {
            Encoding.ASCII.GetBytes(thisisavalue).CopyTo(buffer, start);
        }
    }
}
