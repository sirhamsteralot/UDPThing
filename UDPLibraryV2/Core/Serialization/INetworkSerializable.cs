﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Serialization
{
    public interface INetworkSerializable
    {
        public short TypeId { get; }
        public short MinimumBufferSize { get; }

        void Serialize(byte[] buffer, int start);

        void Deserialize(byte[] buffer, int start);
    }
}