using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Serialization;
using UDPLibraryV2.EndPoint.Messages;
using UDPLibraryV2.EndPoint.Replication.Messages;
using UDPLibraryV2.RPC;
using UDPLibraryV2.RPC.Attributes;

namespace UDPLibraryV2.EndPoint.Replication
{
    public class ReplicationService
    {
        private RPCService rpcService;
        private int timeOutMs;

        private static Dictionary<short, Dictionary<short, INetworkSerializable>> PullTypeInstanceDictionary;

        public ReplicationService(RPCService rpcService, int timeOutMs = 1000)
        {
            this.rpcService = rpcService;
            this.timeOutMs = timeOutMs;

            PullTypeInstanceDictionary = new();
        }

        public void UpdateInstance<T>(short typeId, short instanceId, T value) where T : unmanaged
        {
            UnmanagedSerializerWrapper<T> wrapped = new UnmanagedSerializerWrapper<T>(value, typeId);

            Dictionary<short, INetworkSerializable> InstanceValues;

            if (!PullTypeInstanceDictionary.TryGetValue(typeId, out InstanceValues))
            {
                InstanceValues = new Dictionary<short, INetworkSerializable>();
                PullTypeInstanceDictionary[typeId] = InstanceValues;
            }

            InstanceValues[instanceId] = wrapped;
        }

        public async Task<T> GetRemoteValue<T>(IPEndPoint remote, short requestedTypeId, short instanceId) where T : unmanaged
        {
            ValueRequest request = new ValueRequest(requestedTypeId, instanceId);

            ValueResponse response = await rpcService.CallProcedure<ValueRequest, ValueResponse>(request, false, remote, timeOutMs);

            if (response.valueSize < 1)
                throw new Exception("Error retrieving value! failed response returned!");

            UnmanagedSerializerWrapper<T> wrapper = new UnmanagedSerializerWrapper<T>();
            wrapper.Deserialize(response.value, 0);

            return wrapper.Value;
        }

        [Procedure(3, typeof(ValueRequest), typeof(ValueResponse))]
        public static IResponse ValueRequestProcedure(IRequest request, IPEndPoint source)
        {
            var valueRequest = (ValueRequest)request;

            if (PullTypeInstanceDictionary.TryGetValue(valueRequest.requestedTypeId, out Dictionary<short, INetworkSerializable> InstanceValues))
            {
                if (InstanceValues.TryGetValue(valueRequest.requestedInstanceId, out INetworkSerializable value))
                {
                    return new ValueResponse(value);
                }
            }

            return ValueResponse.CreateFailedResponse();
        }
    }
}
