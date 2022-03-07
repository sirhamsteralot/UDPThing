using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace UDPLibraryV2.Core.Serialization
{
    public static class ValueSerializer
    {

        public static unsafe void NetworkValueSerialize<T>(T typeToSerialize, byte[] outputBuffer, int offset) where T : unmanaged
        {
            fixed (byte* ptr = &outputBuffer[offset])
            {
                IntPtr intPtr = (IntPtr)ptr;

                Marshal.StructureToPtr(typeToSerialize, intPtr, true);
            }
        }

        public static unsafe T NetworkValueDeSerialize<T>(byte[] inputBuffer, int offset) where T : unmanaged
        {
            fixed (byte* ptr = &inputBuffer[offset])
            {
                IntPtr intPtr = (IntPtr)ptr;

                return Marshal.PtrToStructure<T>(intPtr);
            }
        }
    }
}
