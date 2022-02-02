﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.Core.Serialization
{
    public interface INetworkSerializable
    {
        void Serialize(byte[] buffer, int start);

        void Deserialize(byte[] buffer, int start);
    }
}
