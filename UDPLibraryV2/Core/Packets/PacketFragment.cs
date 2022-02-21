using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Stats;

namespace UDPLibraryV2.Core.Packets
{
    public unsafe struct PacketFragment
    {
        public const int COREHEADERSIZE = 3;
        public const int TYPEHEADERSIZE = 2;
        public const int FRAGMENTEDHEADERSIZE = 10;

        public int FragmentBufferLocation { get; init; } = 0;
        public int FragmentPayloadSize { get; init; } = 0;

        // Contains if this fragment contains certain header values or not for serialization/deserialization
        // |--- 7 ---|--- 6 ---|--- 5 ---|--- 4 ---|--- 3 ---|--- 2 ---|--- 1 ---|--- 0 ---|
        // |    x    |    x    |    x    |    x    |    x    |  Compr  |  Fragd  | TypeID  |
        public FragmentFlags HeaderFlags = 0;       // use only the first byte (0-0)

        public short FragmentSize = COREHEADERSIZE; // 1-2
        public short TypeId = 0;                    // 3-4

        public short FragmentId = 0;                // 5-6
        public int FrameCount = 0;                  // 7-10
        public int FrameIndex = 0;                  // 11-14

        private ArraySegment<byte> _payload;

        public PacketFragment(byte[] receiveBuffer, int startPos)
        {
            short headerSize = COREHEADERSIZE;

            FragmentBufferLocation = startPos;
            HeaderFlags = (FragmentFlags)receiveBuffer[startPos];

            fixed (byte* arrayPtr = &receiveBuffer[startPos])
            {
                FragmentSize = *(short*)(arrayPtr + 1);

                if ((HeaderFlags & FragmentFlags.TypeId) == FragmentFlags.TypeId)
                {
                    TypeId = *(short*)(arrayPtr + 3);
                    headerSize += TYPEHEADERSIZE;
                }

                if ((HeaderFlags & FragmentFlags.Fragmented) == FragmentFlags.Fragmented)
                {
                    FragmentId = *(short*)(arrayPtr + 5);
                    FrameCount = *(int*)(arrayPtr + 7);
                    FrameIndex = *(int*)(arrayPtr + 11);
                    headerSize += FRAGMENTEDHEADERSIZE;
                }
            }

            FragmentPayloadSize = FragmentSize - headerSize;
            FragmentBufferLocation += headerSize;

            _payload = new ArraySegment<byte>(receiveBuffer, startPos + headerSize, FragmentPayloadSize);
        }

        public PacketFragment(ArraySegment<byte> payload, short typeId, short fragmentId, int frameCount, int frameIndex, bool compression) : this(payload, typeId, compression)
        {
            HeaderFlags |= FragmentFlags.Fragmented;
            FragmentId = fragmentId;
            FrameCount = frameCount;
            FrameIndex = frameIndex;

            FragmentSize += FRAGMENTEDHEADERSIZE;
        }

        public PacketFragment(ArraySegment<byte> payload, short typeId, bool compression) : this(payload, compression)
        {
            HeaderFlags |= FragmentFlags.TypeId;
            TypeId = typeId;

            FragmentSize += TYPEHEADERSIZE;
        }

        public PacketFragment(ArraySegment<byte> payload, bool compression)
        {
            if (compression)
            {
                HeaderFlags |= FragmentFlags.Compressed;
            }

            _payload = payload;
            FragmentSize += (short)payload.Count;
        }

        public void WriteToBuffer(byte[] buffer, int startPos)
        {
            int writePos = 0;
            fixed (byte* arrayPtr = &buffer[startPos])
            {
                *(byte*)(arrayPtr) = (byte)HeaderFlags;
                (*(short*)(arrayPtr + 1)) = FragmentSize;
                writePos += 3;

                if ((HeaderFlags & FragmentFlags.TypeId) == FragmentFlags.TypeId)
                {
                    *(short*)(arrayPtr + 3) = TypeId;
                    writePos += 2;
                }

                if ((HeaderFlags & FragmentFlags.Fragmented) == FragmentFlags.Fragmented)
                {
                    *(short*)(arrayPtr + 5) = FragmentId;
                    *(int*)(arrayPtr + 7) = FrameCount;
                    *(int*)(arrayPtr + 11) = FrameIndex;
                    writePos += 10;
                }
            }

            _payload.CopyTo(buffer, startPos + writePos);
        }
    }
}
