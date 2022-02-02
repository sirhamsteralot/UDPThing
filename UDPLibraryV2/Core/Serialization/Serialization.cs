using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;

namespace UDPLibraryV2.Core.Serialization
{
    internal class Serialization
    {

        public object DeserializePayload(CompletePacket packet)
        {
            Type payloadType = TypeProvider.Instance.GetType(packet.TypeId);

            var payloadBytes = packet.GetPayloadBytes();
            ArraySegment<byte> validSegment = new ArraySegment<byte>(payloadBytes, 0, payloadBytes.Length);

            return MessagePackSerializer.Deserialize(payloadType, validSegment);
        }
    }
}
