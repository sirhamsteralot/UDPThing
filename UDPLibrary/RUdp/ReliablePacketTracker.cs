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
    internal class ReliablePacketTracker
    {
        public event Action<NetworkPacket> OnPacketFailedToSend;

        readonly Dictionary<uint, PacketTracker> _packetTrackers;
        int _timeOut;
        int _maxRetries;

        UDPCore _udpEndPoint;

        public ReliablePacketTracker(UDPCore ep, int timeout, int maxRetries)
        {
            _udpEndPoint = ep;
            _packetTrackers = new Dictionary<uint, PacketTracker>();
            _timeOut = timeout;
            _maxRetries = maxRetries;
        }

        public void TrackPacket(NetworkPacket packet, IPEndPoint ep)
        {
            PacketTracker tracker;

            if (_packetTrackers.TryGetValue(packet.packetId, out tracker))
            {
                tracker.timer = new Timer(TimerCallback, tracker, _timeOut, Timeout.Infinite);
                return;
            }

            tracker = new PacketTracker()
            {
                ep = ep,
                packet = packet,
                retryCount = 0,
            };

            tracker.timer = new Timer(TimerCallback, tracker, _timeOut, Timeout.Infinite);

            _packetTrackers[packet.packetId] = tracker;
        }

        public void OnPacketAcknowledged(NetworkPacket packet)
        {
            AckPacket ackPacket = new AckPacket();
            packet.Deserialize(ref ackPacket);

            if(_packetTrackers.TryGetValue(ackPacket.ackPackage, out PacketTracker value)) {
                value.Acknowledged();
                _packetTrackers.Remove(ackPacket.ackPackage);
            }
        }

        private void TimerCallback(object? state)
        {
            var tracker = (PacketTracker)state;

            if (tracker.retryCount++ < _maxRetries)
                _udpEndPoint.SendPacketAsync(tracker.ep, tracker.packet);
            else
                OnPacketFailedToSend(tracker.packet);
        }
    }
}

