using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
        private static Dictionary<short, Dictionary<short, INetworkSerializable>> PushTypeInstanceDictionary;

        public ReplicationService(RPCService rpcService, int timeOutMs = 1000)
        {
            this.rpcService = rpcService;
            this.timeOutMs = timeOutMs;

            PullTypeInstanceDictionary = new();
            PushTypeInstanceDictionary = new();
        }

        public void UpdatePullValue<T>(short typeId, short instanceId, T value) where T : unmanaged
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

        public T GetPushValue<T>(short typeId, short instanceId) where T : unmanaged
        {
            UnmanagedSerializerWrapper<T> wrapped = (UnmanagedSerializerWrapper<T>)PushTypeInstanceDictionary[typeId][instanceId];

            return wrapped.Value;
        }

        public void UpdatePushValue<T>(short typeId, short instanceId, T value) where T : unmanaged
        {
            UnmanagedSerializerWrapper<T> wrapped = new UnmanagedSerializerWrapper<T>(value, typeId);

            Dictionary<short, INetworkSerializable> InstanceValues;

            if (!PushTypeInstanceDictionary.TryGetValue(typeId, out InstanceValues))
            {
                InstanceValues = new Dictionary<short, INetworkSerializable>();
                PushTypeInstanceDictionary[typeId] = InstanceValues;
            }

            InstanceValues[instanceId] = wrapped;
        }

        public async ValueTask<T> RequestRemoteValue<T>(IPEndPoint remote, short requestedTypeId, short instanceId) where T : unmanaged
        {
            ValueRequest request = new ValueRequest(requestedTypeId, instanceId);

            ValueResponse response = await rpcService.CallProcedure<ValueRequest, ValueResponse>(request, false, remote, timeOutMs);

            if (response.valueSize < 1)
                throw new Exception("Error retrieving value! failed response returned!");

            UnmanagedSerializerWrapper<T> wrapper = new UnmanagedSerializerWrapper<T>();
            wrapper.Deserialize(response.value, 0);

            return wrapper.Value;
        }

        public async ValueTask<bool> Pushvalue<T>(T value, short typeId, short instanceId, IPEndPoint remote) where T : unmanaged
        {
            try
            {
                UnmanagedSerializerWrapper<T> wrapped = new UnmanagedSerializerWrapper<T>(value, typeId);
                int size = Marshal.SizeOf(value);
                byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
                wrapped.Serialize(buffer, 0);

                ValuePushRequest request = new ValuePushRequest(instanceId, typeId, buffer);
                ValuePushResponse response = await rpcService.CallProcedure<ValuePushRequest, ValuePushResponse>(request, false, remote, timeOutMs);

                ArrayPool<byte>.Shared.Return(buffer);
                return response.successFlags == 1;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
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

        [Procedure(5, typeof(ValuePushRequest), typeof(ValuePushResponse))]
        public static IResponse ValuePushProcedure(IRequest request, IPEndPoint source)
        {
            ValuePushRequest pushRequest = (ValuePushRequest)request;

            if (PushTypeInstanceDictionary.TryGetValue(pushRequest.valueTypeId, out Dictionary<short, INetworkSerializable> instances)) {
                if (instances.TryGetValue(pushRequest.valueInstanceId, out INetworkSerializable instance))
                {
                    instance.Deserialize(pushRequest.value, 0);
                    instances[pushRequest.valueInstanceId] = instance;

                    return new ValuePushResponse()
                    {
                        successFlags = 0x01
                    };
                }
            }

            return new ValuePushResponse()
            {
                successFlags = 0x00
            };
        }
    }
}
