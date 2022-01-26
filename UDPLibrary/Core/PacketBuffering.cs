using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UDPLibrary.Packets;

namespace UDPLibrary.Core
{
    internal class PacketBuffering
    {
        private bool _enabled;

        private readonly int _dataRate;
        private readonly int _rateTMs;

        private UDPCore _endpoint;

        public ConcurrentDictionary<IPEndPoint, Tuple<uint, CompositePacket, byte>> _bufferedPackets;

        public PacketBuffering(UDPCore endPoint, int dataRate)
        {
            _dataRate = dataRate;
            _endpoint = endPoint;

            if (dataRate < 1)
                throw new ArgumentOutOfRangeException("dataRate cannot be 0 or lower!");

            if (dataRate > 1000)
                throw new ArgumentOutOfRangeException("dataRate cannot be greater than 1000!");

            _rateTMs = (int)(1.0 / dataRate * 1000);

            _bufferedPackets = new ConcurrentDictionary<IPEndPoint, Tuple<uint, CompositePacket, byte>>();
        }

        public void StartBuffer()
        {
            _enabled = true;

            BufferLoop();
        }

        public void QueuePacket(IPEndPoint target, INetworkPacket packetToSend, uint sessionId, byte sessionSeq)
        {
            lock (_bufferedPackets)
            {
                if (_bufferedPackets.TryGetValue(target, out Tuple<uint, CompositePacket, byte> packet))
                {
                    packet.Item2.AddPacket(packetToSend);
                }
                else
                {
                    var tempPacket = new CompositePacket();
                    tempPacket.AddPacket(packetToSend);

                    _bufferedPackets.TryAdd(target, Tuple.Create(sessionId, tempPacket, sessionSeq));
                }
            }
        }

        private async Task BufferLoop()
        {
            while (_enabled)
            {
                var msToWait = _rateTMs;
                var watch = Stopwatch.StartNew();

                lock (_bufferedPackets)
                {
                    if (_bufferedPackets.Count > 0)
                    {
                        foreach (var packet in _bufferedPackets)
                        {
                            _endpoint.SendPacketAsync(packet.Key, packet.Value.Item2, false, packet.Value.Item1, packet.Value.Item3).ContinueWith(x => Console.WriteLine(x.Exception), TaskContinuationOptions.OnlyOnFaulted);
                        }

                        _bufferedPackets.Clear();
                    }
                }

                int elapsed = (int)watch.ElapsedMilliseconds;
                if (elapsed < _rateTMs)
                    msToWait = _rateTMs - elapsed;
                else
                    msToWait = 0;

                await Task.Delay(msToWait);
            }
        }
    }
}
