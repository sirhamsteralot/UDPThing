using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.EndPoint;

namespace UDPServerV2
{
    public class EndpointTests
    {
        LocalEndPoint localEP;

        public async void StartServer()
        {
            IPEndPoint listenEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            localEP = new LocalEndPoint(listenEP);

            localEP.Start();
        }

        public async void StartClient()
        {
            IPEndPoint listenEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            localEP = new LocalEndPoint(listenEP);

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);


            localEP.Start();

            var ep = await localEP.Connect(remoteEP);

            Console.WriteLine("finished");
        }
    }
}
