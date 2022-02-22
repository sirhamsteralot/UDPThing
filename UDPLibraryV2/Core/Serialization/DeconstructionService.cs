using K4os.Compression.LZ4;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.Stats;

namespace UDPLibraryV2.Core.Serialization
{
    internal class DeconstructionService
    {
        int _maximumFragmentSize;

        public DeconstructionService(int maximumFragmentSize)
        {
            _maximumFragmentSize = maximumFragmentSize;
        }

        public IEnumerable<PacketFragment> CreateFragments(byte[] payload, short typeId, bool compression, InternalStreamTracker streamTracker = null)
        {
            int bytesLeft = payload.Length;
            byte[] sendBytes = payload;

            if (compression)
            {
                bytesLeft = Compress(payload, out sendBytes);
                
            }

            int messageLength = bytesLeft;

            if (streamTracker != null)
            {
                streamTracker.PayloadSize = payload.Length;
                streamTracker.CompressedSize = messageLength;
            }

            if ((bytesLeft + PacketFragment.TYPEHEADERSIZE + PacketFragment.COREHEADERSIZE) <= _maximumFragmentSize)
            {
                var slice = new ArraySegment<byte>(sendBytes, messageLength - bytesLeft, messageLength);
                var fragment = new PacketFragment(slice, typeId, compression);

                streamTracker?.AddFragment(fragment);

                yield return fragment;
                yield break;
            }

            int maxFragmentedPayloadSize = _maximumFragmentSize - PacketFragment.COREHEADERSIZE - PacketFragment.TYPEHEADERSIZE - PacketFragment.FRAGMENTEDHEADERSIZE;

            short fragmentId = (short)Random.Shared.Next(short.MinValue, short.MaxValue);
            int frameCount = (bytesLeft + maxFragmentedPayloadSize - 1) / maxFragmentedPayloadSize;
            int frameIndex = 0;

            while (bytesLeft >= 0)
            {
                var slice = new ArraySegment<byte>(sendBytes, messageLength - bytesLeft, Math.Min(bytesLeft, maxFragmentedPayloadSize));
                var fragment = new PacketFragment(slice, typeId, fragmentId, frameCount, frameIndex, compression);

                streamTracker?.AddFragment(fragment);

                frameIndex++;
                bytesLeft -= maxFragmentedPayloadSize;

                yield return fragment;
            }
        }

        private int Compress(byte[] toCompress, out byte[] targetArray)
        {
            targetArray = LZ4Pickler.Pickle(toCompress, LZ4Level.L00_FAST);

            return targetArray.Length;
        }
    }
}
