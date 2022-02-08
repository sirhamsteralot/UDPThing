using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Core.PacketQueueing
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

        bool active = true;

        public PacketSender(UDPCore udpCore, int maximumPackageSize, int packetRate)
        {
            _udpCore = udpCore;
            _maximumPackageSize = maximumPackageSize;

            _packetRate = packetRate;
            _delayMs = 1000 / packetRate;

            _sendQueue = new ConcurrentDictionary<short, SendQueue>();
            _sendTargets = new ConcurrentDictionary<short, SendTarget>();

            _sendBuffer = new byte[maximumPackageSize];
        }

        public short OpenStream(IPEndPoint endPoint)
        {
            short streamCode = _udpCore.GetStreamCodeLock();

            _sendTargets[streamCode] = new SendTarget(endPoint);
            _sendQueue[streamCode] = new SendQueue();

            return streamCode;
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
                    if (_sendQueue[keyvaluepair.Key].Count < 1)
                        continue;

                    PrepareNextPacket(0, keyvaluepair.Key);
                    _udpCore.SendBytesAsync(_sendBuffer, _sendBufferSize, keyvaluepair.Value.endPoint).ContinueWith(x => throw x.Exception, TaskContinuationOptions.OnlyOnFaulted);
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

                if (streamQueue.TryPeek(priority, out fragmentToAdd))
                {
                    if (remainingSize - fragmentToAdd.FragmentSize > 0)
                    {
                        streamQueue.TryDeQueue(priority, out fragmentToAdd);

                        packet.AddFragment(fragmentToAdd);
                        remainingSize -= fragmentToAdd.FragmentSize;
                    } else
                    {
                        break;
                    }
                }
                else
                {
                    priority++;
                }
            } while (remainingSize > 0 && priority <= SendPriority.Low);

            _sendBufferSize = packet.SerializeToBuffer(_sendBuffer);
        }
    }
}
