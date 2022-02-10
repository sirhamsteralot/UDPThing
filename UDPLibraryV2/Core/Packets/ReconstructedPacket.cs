using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Packets
{
    public class ReconstructedPacket
    {
        public FragmentFlags Flags { get; init; }
        public short TypeId { get; init; }
        public short StreamId { get; init; }

        ArraySegment<byte>[]? totalPacketData;
        int progress = 0;
        bool completed = false;
        int totalSize = 0;

        byte[] payload;

        public ReconstructedPacket(int totalSize, FragmentFlags flags, short typeId, short streamId)
        {
            StreamId = streamId;
            Flags = flags;
            TypeId = typeId;
            totalPacketData = new ArraySegment<byte>[totalSize];
        }

        public ReconstructedPacket(byte[] payload, FragmentFlags flags, short typeId, short streamId)
        {
            StreamId = streamId;
            Flags = flags;
            TypeId = typeId;
            this.payload = payload;
        }

        public bool AddSegment(ArraySegment<byte> segment, int index)
        {
            if (totalPacketData[index] != null)
                return completed;

            totalPacketData[index] = segment;
            totalSize += segment.Count;
            progress++;

            completed = (progress == totalPacketData.Length);

            return completed;
        }

        public byte[] GetPayloadBytes()
        {
            if (payload != null)
                return payload;

            payload = new byte[totalSize];

            int currentIndex = 0;
            for (int i = 0; i < totalPacketData.Length; i++)
            {
                totalPacketData[i].CopyTo(payload, currentIndex);
                currentIndex += totalPacketData[i].Count;
            }

            return payload;
        }
    }
}
