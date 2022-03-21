using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.EndPoint;
using UDPLibraryV2.EndPoint.Replication;

namespace UDPServerV2
{
    public class API_Concept // concept for generated code
    {
        private LocalEndPoint _endPoint;
        private RemoteEndPoint _remoteEndPoint;

        private PropertyData propertyData_RandomIntThingPushValue = new PropertyData { typeId = 100, instanceId = 0};
        public int RandomIntThingPushValue
        {
            get
            {
                // get value from local cache
                return _endPoint.ReplicationService.GetPushValue<int>(propertyData_RandomIntThingPushValue.typeId, propertyData_RandomIntThingPushValue.instanceId);
            }
            set
            {
                // update locally
                _endPoint.ReplicationService.UpdatePushValue(propertyData_RandomIntThingPushValue.typeId, propertyData_RandomIntThingPushValue.instanceId, value);
                // push change to remote
                _endPoint.ReplicationService.Pushvalue(value, propertyData_RandomIntThingPushValue.typeId, propertyData_RandomIntThingPushValue.instanceId, _remoteEndPoint.ip).ContinueWith(x => Console.Write(x.Exception), TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private PropertyData propertyData_RandomIntThingPullValue = new PropertyData { typeId = 100, instanceId = 1 };
        public Task<int> RandomIntThingPullValue
        {
            get
            {
                // pull value from remote
                return _endPoint.ReplicationService.RequestRemoteValue<int>(_remoteEndPoint.ip, propertyData_RandomIntThingPullValue.typeId, propertyData_RandomIntThingPullValue.instanceId);
            }
            set
            {
                // update local cache
                _endPoint.ReplicationService.UpdatePullValue<int>(propertyData_RandomIntThingPullValue.typeId, propertyData_RandomIntThingPullValue.instanceId, value.Result);
            }
        }

        public API_Concept()
        {
            RandomIntThingPullValue = Task.FromResult(2); // SAD :(
        }
    }
}
