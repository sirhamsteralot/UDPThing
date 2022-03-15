﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.RPC;

namespace UDPLibraryV2.EndPoint.Messages
{
    internal class ConnectionRequest : IRequest
    {
        public short ResponseTypeId => 2;
        public short TypeId => 1;

        public short RequiredSendBufferSize => 6;

        public short ConnectionVersion;

        public Permissions RequestedPermissions;

        public unsafe void Deserialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                ConnectionVersion = *(short*)ptr;
                RequestedPermissions = *(Permissions*)(ptr + 2);
            }
        }

        public unsafe void Serialize(byte[] buffer, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                *(short*)ptr = ConnectionVersion;
                *(Permissions*)(ptr + 2) = RequestedPermissions;
            }
        }
    }
}