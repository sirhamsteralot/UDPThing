using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary
{
    public interface INetworkPacket
    {
        public void Serialize(byte[] buffer, int start);
        public void Deserialize(byte[] payload, int start, int length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetType();
    }
}
