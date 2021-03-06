using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Serialization;
using UDPLibraryV2.RPC;

namespace UDPServerV2.ReqRes
{
    public class HelloWorldRequest : IRequest
    {
        public short ResponseTypeId => 7;

        public short TypeId => 6;

        public short RequiredSendBufferSize => 1;

        public void Deserialize(byte[] buffer, int start)
        {
            return;
        }

        public void Serialize(byte[] buffer, int start)
        {
            return;
        }
    }

    public class HelloWorldResponse : IResponse
    {
        public string ReturnedData;

        public bool DoCompress => true;

        public short TypeId => 7;

        public short RequiredSendBufferSize => (short)Encoding.UTF8.GetByteCount(ReturnedData);

        public void Deserialize(byte[] buffer, int start)
        {
            ReturnedData = Encoding.UTF8.GetString(buffer);
        }

        public void Serialize(byte[] buffer, int start)
        {
            Encoding.UTF8.GetBytes(ReturnedData).CopyTo(buffer, start);
        }
    }
}
