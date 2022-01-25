using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UDPLibrary.Packets;

namespace UDPLibrary.RUdp
{
    internal class PacketTracker
    {
        public NetworkPacket packet;
        public IPEndPoint ep;
        public int retryCount;
        public bool acknowledged;
        public Timer timer;

        public void Acknowledged()
        {
            acknowledged = true;
            timer.Dispose();
            timer = null;
        }
    }
}
