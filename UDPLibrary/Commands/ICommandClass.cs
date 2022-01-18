using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibrary.Commands
{
    public interface ICommandClass
    {
        public void RegisterCommands(CommandManager manager);
    }
}
