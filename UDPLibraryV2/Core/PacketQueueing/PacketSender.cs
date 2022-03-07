using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.Utils;

namespace UDPLibraryV2.Core.PacketQueueing
{
    internal class PacketSender
    {
        public int TargetSendRate { get; init; }

        ObjectPoolList<NetworkPacket> _networkPacketPool;

        UDPCore _udpCore;
        Dictionary<IPEndPoint, SendQueue> _sendQueue;
        Thread _networkingThread;

        int _maximumPayloadSize;

        byte[] _sendBuffer;
        int _sendBufferSize;

        Stopwatch _sendTimer;
        
        int _delayMs;
        int _waitTime;

        bool active = true;

        public PacketSender(UDPCore udpCore, int maximumPayloadSize, int sendRate)
        {
            _udpCore = udpCore;
            _maximumPayloadSize = maximumPayloadSize;

            TargetSendRate = sendRate;
            _delayMs = 1000 / sendRate;

            _sendQueue = new Dictionary<IPEndPoint, SendQueue>();

            _sendTimer = Stopwatch.StartNew();

            _sendBuffer = new byte[maximumPayloadSize];

            _networkPacketPool = new ObjectPoolList<NetworkPacket>(() => new NetworkPacket());
        }

        public void QueueFragment(in PacketFragment fragment, SendPriority priority, IPEndPoint remote)
        {
            lock (_sendQueue)
            {
                _sendQueue.TryAdd(remote, new SendQueue());
                _sendQueue[remote].Queue(fragment, priority);
            }
        }

        public void StartSending()
        {
            _networkingThread = new Thread(SendNetworkMessages);
            _networkingThread.Start();
        }

        private void SendNetworkMessages()
        {
            try
            {
                while (active)
                {
                    long startTime = _sendTimer.ElapsedMilliseconds;

                    SendTick();

                    _waitTime = _delayMs - (int)(_sendTimer.ElapsedMilliseconds - startTime);

                    if (_waitTime > 0)
                        Thread.Sleep(_waitTime);
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void SendTick()
        {
            foreach (var keyvaluepair in _sendQueue)
            {
                if (keyvaluepair.Value.Count < 1)
                    continue;

                PrepareNextPacket(0, keyvaluepair.Value);
                _udpCore.SendBytesAsync(_sendBuffer, _sendBufferSize, keyvaluepair.Key);
            }
        }

        private void PrepareNextPacket(PacketFlags flags, SendQueue endPointQueue)
        {
            NetworkPacket packet = _networkPacketPool.Get();
            packet.SetPacketHeaders(flags, endPointQueue.SequenceByte++, 0);
            int remainingSize = _maximumPayloadSize;

            SendPriority priority = SendPriority.High;

            do
            {
                PacketFragment fragmentToAdd;

                if (endPointQueue.TryPeek(priority, out fragmentToAdd))
                {
                    if (remainingSize - fragmentToAdd.FragmentSize >= 0)
                    {
                        endPointQueue.TryDeQueue(priority, out fragmentToAdd);

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

            _networkPacketPool.Return(packet);
        }
    }
}
