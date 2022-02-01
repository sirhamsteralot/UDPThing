using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Core
{
    internal class IncomingFactory
    {

        public NetworkPacket ConstructPacket(byte[] receiveBuffer)
        {
            var packet = new NetworkPacket(receiveBuffer);

            return packet;
        }
    }
}
