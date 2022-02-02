using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Packets
{
    internal class PacketSender
    {
        public double LoadPercentage;

        UDPCore _udpCore;
        ConcurrentDictionary<short, SendQueue> _sendQueue;
        ConcurrentDictionary<short, SendTarget> _sendTargets;
        int _maximumPackageSize;

        byte[] _sendBuffer;
        int _sendBufferSize;

        int _packetRate;
        int _delayMs;

        bool active;

        public PacketSender(UDPCore udpCore, int maximumPackageSize, int packetRate)
        {
            _udpCore = udpCore;
            _maximumPackageSize = maximumPackageSize;

            _packetRate = packetRate;
            _delayMs = (1 * 10^3) / packetRate;

            _sendQueue = new ConcurrentDictionary<short, SendQueue>();
            _sendTargets = new ConcurrentDictionary<short, SendTarget>();

            _sendBuffer = new byte[maximumPackageSize];
        }

        public void QueueFragment(short streamId, PacketFragment fragment, SendPriority priority)
        {
            _sendQueue[streamId].Queue(fragment, priority);
        }

        public async Task SendNetworkMessages()
        {
            while (active)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                foreach (var keyvaluepair in _sendTargets)
                {
                    PrepareNextPacket(0, keyvaluepair.Key);
                    _udpCore.SendBytes(_sendBuffer, _sendBufferSize, keyvaluepair.Value.endPoint);
                }
                stopwatch.Stop();
                int elapsed = (int)stopwatch.ElapsedMilliseconds;
                LoadPercentage = elapsed / _delayMs;

                await Task.Delay(_delayMs - elapsed);
            }
        }

        private void PrepareNextPacket(PacketFlags flags, short streamId)
        {
            NetworkPacket packet = new NetworkPacket(flags, _sendTargets[streamId].sequenceByte++, streamId);
            int remainingSize = _maximumPackageSize - NetworkPacket.packetHeaderSize;

            var streamQueue = _sendQueue[streamId];

            SendPriority priority = SendPriority.High;

            do
            {
                PacketFragment fragmentToAdd;

                if (streamQueue.TryDeQueue(priority, out fragmentToAdd))
                {
                    packet.AddFragment(fragmentToAdd);
                    remainingSize -= fragmentToAdd.FragmentSize;
                } else
                {
                    priority++;
                }
            } while (remainingSize > 0 && priority <= SendPriority.Low);

            _sendBufferSize = packet.SerializeToBuffer(_sendBuffer);
        }
    }
}
