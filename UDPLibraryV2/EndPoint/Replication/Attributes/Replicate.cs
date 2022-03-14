using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.EndPoint.Replication.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class Replicate : Attribute
    {
        ReplicationMode ReplicationMode { get; set; } = ReplicationMode.Push;

    }
}
