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
        public int TargetSendRate { get; init; }

        UDPCore _udpCore;
        ConcurrentDictionary<short, SendQueue> _sendQueue;
        ConcurrentDictionary<short, SendTarget> _sendTargets;
        int _maximumPayloadSize;

        byte[] _sendBuffer;
        int _sendBufferSize;

        Stopwatch _sendTimer;
        
        int _delayMs;

        bool active = true;

        public PacketSender(UDPCore udpCore, int maximumPayloadSize, int sendRate)
        {
            _udpCore = udpCore;
            _maximumPayloadSize = maximumPayloadSize;

            TargetSendRate = sendRate;
            _delayMs = 1000 / sendRate;

            _sendQueue = new ConcurrentDictionary<short, SendQueue>();
            _sendTargets = new ConcurrentDictionary<short, SendTarget>();

            _sendTimer = Stopwatch.StartNew();

            _sendBuffer = new byte[maximumPayloadSize];
        }

        public short OpenStream(IPEndPoint endPoint)
        {
            short streamCode = _udpCore.GetStreamCodeLock();

            _sendTargets[streamCode] = new SendTarget(endPoint);
            _sendQueue[streamCode] = new SendQueue();

            return streamCode;
        }

        public void OpenStream(IPEndPoint endPoint, short streamId)
        {
            _udpCore.LockStreamCode(streamId);

            _sendTargets[streamId] = new SendTarget(endPoint);
            _sendQueue[streamId] = new SendQueue();
        }

        public void CloseStream(short streamId)
        {
            _sendTargets.TryRemove(streamId, out _);
            _sendQueue.TryRemove(streamId, out _);
        }

        public void QueueFragment(short streamId, PacketFragment fragment, SendPriority priority)
        {
            _sendQueue[streamId].Queue(fragment, priority);
        }

        public async Task SendNetworkMessages()
        {
            while (active)
            {
                long startTime = _sendTimer.ElapsedMilliseconds;

                SendTick();

                int waitTime = _delayMs - (int)(_sendTimer.ElapsedMilliseconds - startTime);

                if (waitTime > 0)
                    await Task.Delay(waitTime);
            }
        }

        private void SendTick()
        {
            foreach (var keyvaluepair in _sendTargets)
            {
                if (_sendQueue[keyvaluepair.Key].Count < 1)
                    continue;

                PrepareNextPacket(0, keyvaluepair.Key);
                _udpCore.SendBytesAsync(_sendBuffer, _sendBufferSize, keyvaluepair.Value.endPoint).ContinueWith(x => throw x.Exception, TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private void PrepareNextPacket(PacketFlags flags, short streamId)
        {
            NetworkPacket packet = new NetworkPacket(flags, _sendTargets[streamId].sequenceByte++, streamId);
            int remainingSize = _maximumPayloadSize;

            var streamQueue = _sendQueue[streamId];

            SendPriority priority = SendPriority.High;

            do
            {
                PacketFragment fragmentToAdd;

                if (streamQueue.TryPeek(priority, out fragmentToAdd))
                {
                    if (remainingSize - fragmentToAdd.FragmentSize >= 0)
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
