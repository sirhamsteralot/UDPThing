using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPThing
{
    public class UDPListener
    {
        private const int _listenPort = 11000;

        UdpClient listener = new UdpClient(_listenPort);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, _listenPort);

        public UDPListener()
        {
        }

        public void StartListener()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);

                    Console.WriteLine($"Received broadcast from {groupEP} :");
                    Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }
        }
    }
}
