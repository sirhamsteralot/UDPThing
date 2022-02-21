using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;
using MessagePack;
using K4os.Compression.LZ4;
using System.Buffers;

namespace UDPLibraryV2.Core.Serialization
{
    internal class ReconstructionService : NetworkServiceBase
    {
        public event Action<ReconstructedPacket, IPEndPoint?> OnPayloadReconstructed;

        Dictionary<short, ReconstructedPacket> _fragmentedFragments;

        public ReconstructionService(UDPCore core) : base(core)
        {
            _fragmentedFragments = new Dictionary<short, ReconstructedPacket>();
        }

        public override void OnMessageReceivedRaw(NetworkPacket incomingPacket, IPEndPoint? sourceEndPoint)
        {
            int currentLocation = NetworkPacket.HeaderSize;

            if (incomingPacket.Fragments == null)
                return;

            foreach (PacketFragment fragment in incomingPacket.Fragments)
            {
                if ((fragment.HeaderFlags & FragmentFlags.Fragmented) != FragmentFlags.Fragmented)
                {
                    byte[] payloadBytes = new byte[fragment.FragmentPayloadSize];
                    byte[] dataBytes = payloadBytes;

                    Buffer.BlockCopy(incomingPacket.Buffer, fragment.FragmentBufferLocation, payloadBytes, 0, fragment.FragmentPayloadSize);

                    if ((fragment.HeaderFlags & FragmentFlags.Compressed) == FragmentFlags.Compressed)
                    {
                        dataBytes = Decompress(payloadBytes);
                    }

                    OnPayloadReconstructed?.Invoke(new ReconstructedPacket(dataBytes, fragment.HeaderFlags, fragment.TypeId, incomingPacket.Streamid), sourceEndPoint);
                    continue;
                }

                ReconstructedPacket completePacket;
                if (!_fragmentedFragments.TryGetValue(fragment.FragmentId, out completePacket))
                {
                    completePacket = new ReconstructedPacket(fragment.FrameCount, fragment.HeaderFlags, fragment.TypeId, incomingPacket.Streamid);
                    _fragmentedFragments[fragment.FragmentId] = completePacket;
                }

                var segment = new ArraySegment<byte>(incomingPacket.Buffer, fragment.FragmentBufferLocation, fragment.FragmentPayloadSize);
                if (completePacket.AddSegment(segment, fragment.FrameIndex))
                {
                    if ((completePacket.Flags & FragmentFlags.Compressed) == FragmentFlags.Compressed)
                    {
                        var payloadBytes = completePacket.GetPayloadBytes();

                        var decompressedBytes = Decompress(payloadBytes);
                        var decompressed = new ReconstructedPacket(decompressedBytes, completePacket.Flags, completePacket.TypeId, incomingPacket.Streamid);

                        OnPayloadReconstructed?.Invoke(decompressed, sourceEndPoint);
                        return;
                    }

                    OnPayloadReconstructed?.Invoke(completePacket, sourceEndPoint);
                }
            }
        }

        private byte[] Decompress(byte[] toDecompress)
        {
            byte[] outputBuffer = ArrayPool<byte>.Shared.Rent(toDecompress.Length * 255);

            int bytesWritten = LZ4Codec.Decode(toDecompress, outputBuffer);

            byte[] output = new byte[bytesWritten];
            Buffer.BlockCopy(outputBuffer, 0, output, 0, bytesWritten);

            ArrayPool<byte>.Shared.Return(output);

            return output;
        }
    }
}
