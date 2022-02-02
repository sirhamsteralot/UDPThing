using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Core.Serialization
{
    internal class DeconstructionService
    {
        int _maximumFragmentPayloadSize;

        public DeconstructionService(int maximumPackageSize)
        {
            _maximumFragmentPayloadSize = maximumPackageSize;
        }

        public IEnumerable<PacketFragment> CreateFragments(byte[] payload, short typeId, bool compression)
        {
            int bytesLeft = payload.Length;

            if (bytesLeft < _maximumFragmentPayloadSize)
            {
                var slice = new ArraySegment<byte>(payload, payload.Length - bytesLeft, payload.Length);
                var fragment = new PacketFragment(slice, typeId, compression);

                yield return fragment;
                yield break;
            }

            short fragmentId = (short)Random.Shared.Next(short.MinValue, short.MaxValue);
            int frameCount = (bytesLeft + _maximumFragmentPayloadSize - 1) / _maximumFragmentPayloadSize;
            int frameIndex = 0;

            while (bytesLeft > 0)
            {
                var slice = new ArraySegment<byte>(payload, payload.Length - bytesLeft, Math.Min(bytesLeft, _maximumFragmentPayloadSize));
                var fragment = new PacketFragment(slice, typeId, fragmentId, frameCount, frameIndex, compression);

                frameIndex++;
                bytesLeft -= _maximumFragmentPayloadSize;

                yield return fragment;
            }
        }
    }
}
