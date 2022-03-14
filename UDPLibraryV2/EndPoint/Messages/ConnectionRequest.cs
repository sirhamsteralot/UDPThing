using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.RPC;

namespace UDPLibraryV2.EndPoint.Messages
{
    internal class ConnectionRequest : IRequest
    {
        public short ResponseTypeId => 1;
        public short TypeId => 0;

        public short MinimumBufferSize => throw new NotImplementedException();



        public void Deserialize(byte[] buffer, int start)
        {
            throw new NotImplementedException();
        }

        public void Serialize(byte[] buffer, int start)
        {
            throw new NotImplementedException();
        }
    }
}
