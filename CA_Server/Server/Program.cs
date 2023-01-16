﻿using Server.Game;
using ServerCore;
using System;
using System.Net;

namespace Server
{
    internal class Program
    {
        private static Listener _listener = new Listener();

        private static void FlushRoom()
        {
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        private static void Main(string[] args)
        {
            RoomManager.Instance.Add();

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            JobTimer.Instance.Push(FlushRoom);

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}