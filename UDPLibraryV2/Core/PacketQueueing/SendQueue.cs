using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Core.PacketQueueing
{
    internal class SendQueue
    {
        public int Count { get; set; }

        public byte SequenceByte { get; set; } = 0;

        private Dictionary<SendPriority, ConcurrentQueue<PacketFragment>> _sendQueue;

        public SendQueue()
        {
            _sendQueue = new Dictionary<SendPriority, ConcurrentQueue<PacketFragment>>();

            var highQueue = new ConcurrentQueue<PacketFragment>();
            _sendQueue.Add(SendPriority.High, highQueue);

            var medQueue = new ConcurrentQueue<PacketFragment>();
            _sendQueue.Add(SendPriority.Medium, medQueue);

            var lowQueue = new ConcurrentQueue<PacketFragment>();
            _sendQueue.Add(SendPriority.Low, lowQueue);
        }

        public void Queue(PacketFragment fragment, SendPriority priority)
        {
            _sendQueue[priority].Enqueue(fragment);
            Count++;
        }

        public bool TryDeQueue(SendPriority priority, out PacketFragment fragment)
        {
            if( _sendQueue[priority].TryDequeue(out fragment))
            {
                Count--;
                return true;
            }
            return false;
        }

        public bool TryPeek(SendPriority priority, out PacketFragment fragment)
        {
            return _sendQueue[priority].TryPeek(out fragment);
        }
    }
}
