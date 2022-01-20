using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Packets
{
    public class CompositePacket : INetworkPacket
    {
        public const int PacketType = 2;

        public int subPacketCount;
        public List<int> packageSizes;
        public List<uint> subPacketTypes;
        public List<INetworkPacket> subPackets;

        public int totalPackageSize;

        public CompositePacket()
        {
            packageSizes = new List<int>();
            subPackets = new List<INetworkPacket>();
            subPacketTypes = new List<uint>();
            totalPackageSize = 4;
        }

        public void AddPacket(INetworkPacket packet)
        {
            subPackets.Add(packet);
            packageSizes.Add(packet.GetSize());
            subPacketTypes.Add(packet.GetPacketType());
            subPacketCount++;
            totalPackageSize += packet.GetSize() + 4 + 4;
        }

        public unsafe void Deserialize(byte[] payload, int start, int length)
        {
            fixed (byte* ptr = &payload[start])
            {
                *(int*)ptr = subPacketCount;

                int currentPos = start + 4 + (2 * (subPacketCount * 4));

                for (int i = 0; i < subPacketCount; i++)
                {
                    packageSizes.Add(*(int*)(ptr + 4 + (i * 4)));
                }

                for (int i = 0; i < subPacketCount; i++)
                {
                    uint packetType = *(uint*)(ptr + 4 + (subPacketCount * 4) + (i * 4));
                    subPacketTypes.Add(packetType);

                    var thingToAdd = (INetworkPacket)Activator.CreateInstance(PacketFactory.GetPacketType(packetType));
                    if (thingToAdd == null)
                        throw new Exception("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                    subPackets.Add(thingToAdd);
                }


                for (int i = 0; i < subPacketCount; i++)
                {
                    subPackets[i].Deserialize(payload, currentPos, packageSizes[i]);
                    currentPos += packageSizes[i];
                }
            }
        }

        public uint GetPacketType()
        {
            return PacketType;
        }

        public int GetSize()
        {
            return totalPackageSize;
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                *(int*)ptr = subPacketCount;

                int currentPos = start + 4 + (2 * (subPacketCount * 4));

                for (int i = 0; i < subPacketCount; i++)
                {
                    *(int*)(ptr + 4 + (i * 4)) = packageSizes[i];
                }

                for (int i = 0; i < subPacketCount; i++)
                {
                    *(uint*)(ptr + 4 + (subPacketCount * 4) + (i * 4)) = subPacketTypes[i];
                }

                for (int i = 0; i < subPacketCount; i++)
                {
                    subPackets[i].Serialize(buffer, currentPos);
                    currentPos += subPackets[i].GetSize();
                }
            }
        }
    }
}
