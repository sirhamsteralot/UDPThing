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

namespace UDPLibrary
{
    internal class PacketBuffering
    {
        private bool _enabled;

        private readonly int _dataRate;
        private readonly int _rateTMs;

        private UDPCore _endpoint;

        public ConcurrentDictionary<IPEndPoint, CompositePacket> _bufferedPackets;

        public PacketBuffering(UDPCore endPoint, int dataRate)
        {
            _dataRate = dataRate;
            _endpoint = endPoint;

            if (dataRate < 1)
                throw new ArgumentOutOfRangeException("dataRate cannot be 0 or lower!");

            if (dataRate > 1000)
                throw new ArgumentOutOfRangeException("dataRate cannot be greater than 1000!");

            _rateTMs = (int)((1.0 / dataRate) * 1000);

            _bufferedPackets = new ConcurrentDictionary<IPEndPoint,CompositePacket>();
        }

        public void StartBuffer()
        {
            _enabled = true;

            BufferLoop();
        }

        public void QueuePacket(IPEndPoint target, INetworkPacket packetToSend)
        {
            lock (_bufferedPackets)
            {
                if (_bufferedPackets.TryGetValue(target, out CompositePacket packet))
                {
                    packet.AddPacket(packetToSend);
                }
                else
                {
                    var tempPacket = new CompositePacket();
                    tempPacket.AddPacket(packetToSend);

                    _bufferedPackets.TryAdd(target, tempPacket);
                }
            }
        }

        private async Task BufferLoop()
        {   
            while(_enabled)
            {
                var msToWait = _rateTMs;
                var watch = Stopwatch.StartNew();

                lock(_bufferedPackets)
                {
                    if (_bufferedPackets.Count > 0)
                    {
                        foreach (var packet in _bufferedPackets)
                        {
                            _endpoint.SendPacketAsync(packet.Key, packet.Value, false).ContinueWith(x => Console.WriteLine(x.Exception), TaskContinuationOptions.OnlyOnFaulted);
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
