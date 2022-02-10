using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;
using MessagePack;

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
                    byte[] payloadBytes = new byte[fragment.FragmentSize];
                    Buffer.BlockCopy(incomingPacket.Buffer, currentLocation, payloadBytes, 0, payloadBytes.Length);
                    OnPayloadReconstructed?.Invoke(new ReconstructedPacket(payloadBytes, fragment.HeaderFlags, fragment.TypeId, incomingPacket.Streamid), sourceEndPoint);
                    continue;
                }

                ReconstructedPacket completePacket;
                if (!_fragmentedFragments.TryGetValue(fragment.FragmentId, out completePacket))
                {
                    completePacket = new ReconstructedPacket(fragment.FrameCount, fragment.HeaderFlags, fragment.TypeId, incomingPacket.Streamid);
                    _fragmentedFragments[fragment.FragmentId] = completePacket;
                }

                var segment = new ArraySegment<byte>(incomingPacket.Buffer, fragment.FragmentBufferLocation, fragment.FragmentSize);
                if (completePacket.AddSegment(segment, fragment.FrameIndex))
                {
                    var payloadBytes = completePacket.GetPayloadBytes();
                    OnPayloadReconstructed?.Invoke(completePacket, sourceEndPoint);
                }
            }
        }
    }
}
