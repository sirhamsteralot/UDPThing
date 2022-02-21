using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Serialization;

namespace UDPLibraryV2.Core.Packets
{
    internal class ReliablePacketSender
    {
        private UDPCore _core;
        private DeconstructionService _deconstructionService;

        private int _maximumPayloadSize;

        public ReliablePacketSender(UDPCore core, DeconstructionService deconstructionService, int maximumPayloadSize)
        {
            _core = core;
            _deconstructionService = deconstructionService;
            _maximumPayloadSize = maximumPayloadSize;
        }

        public async Task<bool> SendSerializableReliable(INetworkSerializable serializable, bool compression, IPEndPoint remote, int retries, int retryDelay)
        {
            byte[] serializationBuffer = ArrayPool<byte>.Shared.Rent(serializable.MinimumBufferSize);
            byte[] sendBuffer = ArrayPool<byte>.Shared.Rent(_maximumPayloadSize);

            serializable.Serialize(serializationBuffer, 0);

            var fragments = _deconstructionService.CreateFragments(serializationBuffer, serializable.TypeId, compression);

            short streamId = (short)Random.Shared.Next();

            Dictionary<byte, NetworkPacket> sentPackets = new Dictionary<byte, NetworkPacket>(serializable.MinimumBufferSize + _maximumPayloadSize - 1 / _maximumPayloadSize);

            TaskCompletionSource<bool> completed = new TaskCompletionSource<bool>();
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Action<NetworkPacket, IPEndPoint?> messageReceivedCallback = (packet, ep) =>
            {
                if (packet.Streamid == streamId && (packet.Flags | PacketFlags.Acknowledge) == PacketFlags.Acknowledge)
                {
                    if (sentPackets.Remove(packet.Seq))
                    {
                        if (sentPackets.Count < 1)
                        {
                            completed.TrySetResult(true);
                            tokenSource.Cancel();

                        }
                    }
                }
            };

            _core.OnMessageReceivedRaw += messageReceivedCallback;
            byte seq = 0;

            foreach (var fragment in fragments)
            {
                NetworkPacket packet = new NetworkPacket(PacketFlags.Reliable, seq, streamId);
                packet.AddFragment(fragment);

                await _core.SendPacketAsync(packet, remote, sendBuffer);

                sentPackets.Add(packet.Seq, packet);
                seq++;
            }

            while (retries-- > 0 && !completed.Task.IsCompleted)
            {
                try
                {
                    await Task.Delay(retryDelay, tokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                foreach (var packet in sentPackets.Values)
                {
                    await _core.SendPacketAsync(packet, remote, sendBuffer);
                }
            }

            if (sentPackets.Count > 0)
                completed.TrySetResult(false);

            bool result = await completed.Task;

            // Cleanup
            _core.OnMessageReceivedRaw -= messageReceivedCallback;
            ArrayPool<byte>.Shared.Return(sendBuffer);
            ArrayPool<byte>.Shared.Return(serializationBuffer);

            return result;
        }

    }
}
