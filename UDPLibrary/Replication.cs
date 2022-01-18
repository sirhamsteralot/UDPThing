using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public class Replication
    {
        ClientEndPoint _clientEndPoint;

        public Replication(ClientEndPoint clientEndPoint)
        {
            _clientEndPoint = clientEndPoint;
        }

        public void Replicate()
        {

        }

        private byte[] GetReplicationBytes()
        {

        }
    }
}
