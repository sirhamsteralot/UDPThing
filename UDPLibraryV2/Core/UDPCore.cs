using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.Core.Serialization;

namespace UDPLibraryV2.Core
{
    internal class UDPCore
    {
        private int _listenPort;
        private UdpClient _listener;

        private DeconstructionService _deconstructionService;
        private ReconstructionService _reconstructionService;

        private List<INetworkService> _services;

        public UDPCore(IPEndPoint listenIp)
        {
            _listener = new UdpClient(listenIp);
            _listenPort = ((IPEndPoint)_listener.Client.LocalEndPoint).Port;

            _services = new List<INetworkService>();

            _deconstructionService = new DeconstructionService(NetworkPacket.payloadMaxSize / 2);

            _reconstructionService = new ReconstructionService(this);
            _reconstructionService.OnPayloadReconstructed += ReconstructionService_OnPayloadReconstructed;
            RegisterService(_reconstructionService);
        }

        public void StartListening()
        {
            _listener.BeginReceive(NetworkReceiveCallback, null);
        }

        public void RegisterService(INetworkService service)
        {
            _services.Add(service);
        }

        public void SendBytes(byte[] bytes, int amountToSend, IPEndPoint endpoint)
        {
            _listener.Send(bytes, amountToSend, endpoint);
        }

        private void ReconstructionService_OnPayloadReconstructed(CompletePacket packet, IPEndPoint? sourceEP)
        {
            
        }

        private void NetworkReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint? EP = null;
            byte[] receiveBuffer = _listener.EndReceive(ar, ref EP);

            NetworkPacket packet = new NetworkPacket(receiveBuffer);

            foreach (var service in _services)
                service.OnMessageReceivedRaw(packet, EP);
        }
    }
}
