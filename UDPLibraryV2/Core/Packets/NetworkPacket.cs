using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Packets
{
    internal class NetworkPacket
    {
        const int maximumUdpPacketSize = 256;
        const int udpHeaderSize = 28;
        const int packetHeaderSize = 4;
        const int payloadMaxSize = maximumUdpPacketSize - udpHeaderSize - packetHeaderSize;


        // Contains a number of packet flags for compression, reliable sending etc.
        // |--- 7 ---|--- 6 ---|--- 5 ---|--- 4 ---|--- 3 ---|--- 2 ---|--- 1 ---|--- 0 ---|
        // |    x    |    x    |    x    |    x    |    x    |   Ack   |  Compr  |   Rel   |
        byte _packetFlags; // 1

        byte _packetSeq; // 2
        short _streamId; // 4

        List<Fragment> fragments;

        byte[] bytes;

        internal unsafe NetworkPacket(byte[] receiveBuffer)
        {
            bytes = receiveBuffer;

            fixed (byte* receiveBufferPtr = receiveBuffer)
            {
                _packetFlags = *receiveBufferPtr;
                _packetSeq = *(receiveBufferPtr + 1);
                _streamId = *(short*)(receiveBufferPtr + 2);
            }
        }

        internal NetworkPacket(byte[] sendBuffer, PacketFlags packetFlags, byte packetSeq, short streamId)
        {
            bytes = sendBuffer;

            _packetFlags = (byte)packetFlags;
            _packetSeq = packetSeq;
            _streamId = streamId;
        }

        internal unsafe struct Fragment
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
