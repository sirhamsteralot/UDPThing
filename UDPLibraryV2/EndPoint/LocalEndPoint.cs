using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core;
using UDPLibraryV2.RPC;

namespace UDPLibraryV2.EndPoint
{
    internal class LocalEndPoint
    {

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

        public async Task<RemoteEndPoint> Connect(IPEndPoint endPoint)
        {
            
        }
    }
}
