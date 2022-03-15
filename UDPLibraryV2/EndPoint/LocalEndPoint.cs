using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core;
using UDPLibraryV2.EndPoint.Messages;
using UDPLibraryV2.RPC;
using UDPLibraryV2.RPC.Attributes;

namespace UDPLibraryV2.EndPoint
{
    public class LocalEndPoint
    {
        public const short version = 1;


        private UDPCore _udpCore;
        public UDPCore NetworkingCore => _udpCore;

        private RPCService _rpcService;
        public RPCService RPCService => _rpcService;

        public LocalEndPoint(IPEndPoint listenEndPoint, int sendRate = 64, int maxPayloadSize = 512)
        {
            _udpCore = new UDPCore(listenEndPoint, sendRate, maxPayloadSize);

            _rpcService = new RPCService(_udpCore);
        }

        public void Start()
        {
            _rpcService.FindAndRegisterProcedures();

            _udpCore.StartListening();
            _udpCore.StartSending();
        }

        public async Task<RemoteEndPoint?> Connect(IPEndPoint endPoint)
        {
            var request = new ConnectionRequest()
            {
                RequestedPermissions = Permissions.Base,
                ConnectionVersion = version,
            };

            var response = await _rpcService.CallProcedure<ConnectionRequest, ConnectionResponse>(request, false, endPoint, 2500);

            if (response.ConnectionGranted)
            {
                return new RemoteEndPoint();
            }

            return null;
        }

        [Procedure(1, typeof(ConnectionRequest), typeof(ConnectionResponse))]
        public static IResponse ConnectionRequestProcedure(IRequest request, IPEndPoint source)
        {
            var connectionReq = ((ConnectionRequest)request);

            if (connectionReq.ConnectionVersion != version)
            {
                return new ConnectionResponse()
                {
                    GrantedPermissions = Permissions.None,
                    ConnectionGranted = false,
                };
            }

            return new ConnectionResponse() {
                GrantedPermissions = connectionReq.RequestedPermissions,
                ConnectionGranted = true,
            };
        }
    }
}
