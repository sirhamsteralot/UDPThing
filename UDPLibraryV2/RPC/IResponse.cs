﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Serialization;

namespace UDPLibraryV2.RPC
{
    public interface IResponse : INetworkSerializable
    {
        bool DoCompress { get; }
    }
}
