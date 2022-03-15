using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Serialization;

namespace UDPLibraryV2.EndPoint.Replication
{
    public class ReplicableChanges : INetworkSerializable
    {
        public Guid instanceId;

        public short TypeId { get; private set; }

        public short RequiredSendBufferSize {get; private set; }

        public List<INetworkSerializable> Changes { get; private set; } = new List<INetworkSerializable>();

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
