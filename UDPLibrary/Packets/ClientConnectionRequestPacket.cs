using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class ClientConnectionRequestPacket : INetworkPacket
    {
        public int[] _events;
        public int[] _replicables;

        public void Serialize(byte[] buffer, int start)
        {
            Buffer.BlockCopy(_events, 0, buffer, start, _events.Length * sizeof(int));
            Buffer.BlockCopy(_replicables, 0, buffer, start + _events.Length * sizeof(int), _replicables.Length * sizeof(int));
        }

        public void Deserialize(byte[] payload, int start, int length)
        {
            throw new NotImplementedException();
        }

        public int GetSize()
        {
            throw new NotImplementedException();
        }

        public uint GetPacketType()
        {
            throw new NotImplementedException();
        }
    }
}
