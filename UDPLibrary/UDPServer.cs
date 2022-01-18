using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UDPLibrary.Commands;

namespace UDPLibrary
{
    public class UDPServer
    {
        public event Action<byte[], IPEndPoint> OnMessageReceived;

        private readonly int _listenPort;

        private bool _listen = true;
        private UdpClient _listener;
        private IPEndPoint _groupEP;

        private CommandManager _commandManager;

        public UDPServer(int listenPort)
        {
            _listenPort = listenPort;

            _groupEP = new IPEndPoint(IPAddress.Any, _listenPort);
            _listener = new UdpClient(_listenPort);
            _commandManager = new CommandManager();

            OnMessageReceived += _commandManager.OnMessageReceived;

            _commandManager.RegisterCommand(EstablishClientConnection);

            Receive();
        }

        public void StopReceiving()
        {
            _listen = false;
            _listener.Client.Close();
        }

        private void Receive()
        {
            _listener.BeginReceive(MyReceiveCallback, null);
        }

        private void MyReceiveCallback(IAsyncResult result)
        {

            IPEndPoint? EP = null;
            byte[] bytes = _listener.EndReceive(result, ref EP);

            if (EP != null)
                OnMessageReceived(bytes, EP);

            if (_listen)
            {
                Receive();
            }
        }

        private void EstablishClientConnection(byte[] message, IPEndPoint source)
        {

        }
    }
}