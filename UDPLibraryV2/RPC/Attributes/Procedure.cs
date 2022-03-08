using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.RPC.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Procedure : Attribute
    {
        public Type RequestType { get; init; }
        public Type ResponseType { get; init; }
        public short ProcedureId { get; init; }

        public Procedure(short procedureId, Type requestTypeName, Type responseTypeName)
        {
            RequestType = requestTypeName;
            ResponseType = responseTypeName;
            ProcedureId = procedureId;
        }
    }
}
