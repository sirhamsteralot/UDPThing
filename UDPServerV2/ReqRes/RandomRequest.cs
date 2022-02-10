using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.RequestResponse;

namespace UDPServerV2.ReqRes
{
    internal class RandomRequest : IRequest
    {
        public short RequestId => 2;

        public short TypeId => 2;

        public short MinimumBufferSize => 0;

        public void Deserialize(byte[] buffer, int start)
        {
            return;
        }

        public void Serialize(byte[] buffer, int start)
        {
            return;
        }
    }
}
