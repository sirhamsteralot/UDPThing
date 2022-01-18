using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Commands
{
    public class CommandManager
    {
        private List<Action<byte[], IPEndPoint>> _commands = new List<Action<byte[], IPEndPoint>>();

        internal void OnMessageReceived(byte[] message, IPEndPoint source)
        {
            int command = (message[0] << 8) | message[1];

            if (command > 0 && command < _commands.Count)
                _commands[command].Invoke(message, source);
        }

        public void RegisterCommand(Action<byte[], IPEndPoint> action)
        {
            _commands.Add(action);
        }
    }
}
