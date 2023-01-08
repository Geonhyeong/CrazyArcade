using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    internal class Program
    {
        private static Listener _listener = new Listener();

        private static void Main(string[] args)
        {
            PacketManager.Instance.Register();

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("Listening...");

            while (true)
            {
                ;
            }
        }
    }
}