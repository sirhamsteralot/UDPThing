using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Core
{
    internal class SendQueue
    {
        private Dictionary<SendPriority, ConcurrentQueue<PacketFragment>> _sendQueue;

        public SendQueue()
        {
            _sendQueue = new Dictionary<SendPriority, ConcurrentQueue<PacketFragment>>();
        }

        public void Queue(PacketFragment fragment, SendPriority priority)
        {
            ConcurrentQueue<PacketFragment> queue;
            if (!_sendQueue.TryGetValue(priority, out queue))
            {
                queue = new ConcurrentQueue<PacketFragment>();
                _sendQueue.Add(priority, queue);
            }
            
            queue.Enqueue(fragment);
        }

        public bool TryDeQueue(SendPriority priority, out PacketFragment fragment)
        {
            return _sendQueue[priority].TryDequeue(out fragment);
        }

        public bool TryPeek(SendPriority priority, out PacketFragment fragment)
        {
            return _sendQueue[priority].TryPeek(out fragment);
        }
    }
}
