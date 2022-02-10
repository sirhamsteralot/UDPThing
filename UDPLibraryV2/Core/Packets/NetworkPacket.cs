using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Packets
{
    public class NetworkPacket
    {
        public const int HeaderSize = 4;

        public List<PacketFragment> Fragments => fragments;
        public byte[] Buffer => buffer;
        public short Streamid => _streamId;


        // Contains a number of packet flags for compression, reliable sending etc.
        // |--- 7 ---|--- 6 ---|--- 5 ---|--- 4 ---|--- 3 ---|--- 2 ---|--- 1 ---|--- 0 ---|
        // |    x    |    x    |    x    |    x    |    x    |   Ack   |    x    |   Rel   |
        byte _packetFlags; // 0 - 0

        byte _packetSeq; // 1 - 1
        short _streamId; // 2 - 3

        List<PacketFragment> fragments;

        byte[] buffer = null;

        internal unsafe NetworkPacket(byte[] receiveBuffer)
        {
            buffer = receiveBuffer;

            fixed (byte* receiveBufferPtr = receiveBuffer)
            {
                _packetFlags = *receiveBufferPtr;
                _packetSeq = *(receiveBufferPtr + 1);
                _streamId = *(short*)(receiveBufferPtr + 2);
            }

            int bytesLeft = receiveBuffer.Length - HeaderSize;
            while (bytesLeft > 0)
            {
                if (fragments == null)
                    fragments = new List<PacketFragment>();

                var fragment = new PacketFragment(receiveBuffer, receiveBuffer.Length - bytesLeft);
                fragments.Add(fragment);

                bytesLeft -= fragment.FragmentSize;
            }
        }

        public NetworkPacket(PacketFlags packetFlags, byte packetSeq, short streamId)
        {
            _packetFlags = (byte)packetFlags;
            _packetSeq = packetSeq;
            _streamId = streamId;
        }

        internal void AddFragment(PacketFragment fragment)
        {
            if (fragments == null)
                fragments = new List<PacketFragment>();

            fragments.Add(fragment);
        }

        internal unsafe int SerializeToBuffer(byte[] sendBuffer)
        {
            int bytesSerialized = 0;

            fixed (byte* sendBufferPtr = sendBuffer)
            {
                *sendBufferPtr = _packetFlags;
                *(sendBufferPtr + 1) = _packetSeq;
                *(short*)(sendBufferPtr + 2) = _streamId;

                bytesSerialized += 4;
            }

            if (fragments == null)
                return bytesSerialized;

            foreach (var fragment in fragments)
            {
                fragment.WriteToBuffer(sendBuffer, bytesSerialized);
                bytesSerialized += fragment.FragmentSize;
            }

            return bytesSerialized;
        }
    }
}
