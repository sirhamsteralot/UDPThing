using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class GeneralPacket
    {
        const int maximumUdpPacketSize = 256;
        const int udpHeaderSize = 28;
        const int ownSize = 4;
        const int payloadMaxSize = maximumUdpPacketSize - udpHeaderSize - ownSize;
        const int fragmentHeaderSize = 13;

        public int fragmentSizeLeft = payloadMaxSize;

        byte packetSeq; // 1

        // Contains a number of packet flags for compression, reliable sending etc.
        // |--- 7 ---|--- 6 ---|--- 5 ---|--- 4 ---|--- 3 ---|--- 2 ---|--- 1 ---|--- 0 ---|
        // |    x    |    x    |    x    |    x    |    x    |   Ack   |  Compr  |   Rel   |
        byte packetFlags; // 2


        Int16 streamId; // 4

        byte[] bytes;

        Fragment[] fragments;

        public GeneralPacket()
        {
            bytes = new byte[payloadMaxSize];
        }

        public GeneralPacket(byte[] byteBuffer)
        {
            bytes = byteBuffer;
        }

        private void AddFragment(ref Fragment fragment)
        {

            fragmentSizeLeft -= ( fragmentHeaderSize - fragment.Size);
        }

        public unsafe struct Fragment
        {
            // Contains if this fragment contains certain header values or not for serialization/deserialization
            // |--- 7 ---|--- 6 ---|--- 5 ---|--- 4 ---|--- 3 ---|--- 2 ---|--- 1 ---|--- 0 ---|
            // |    x    |    x    |    x    |    x    |  Index  |  Size   | FragID  | TypeID  |
            public byte HeaderFlags;

            public short TypeId; // 3
            public short FragmentId; // 5
            public int Size; // 9
            public int Index; // 13

            public byte* arrayPointer; 


        }
    }
}
