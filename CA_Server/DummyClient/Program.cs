using DummyClient.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace DummyClient
{
    class Program
    {
        static int DummyClientCount { get; } = 200;
        private static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

        static void Tick(int tick = 100)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = tick;
            timer.Elapsed += ((s, e) => { Map.Instance.Update(); });
            timer.AutoReset = true;
            timer.Enabled = true;

            _timers.Add(timer);
        }

        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

            Connector connector = new Connector();

            connector.Connect(endPoint,
                () => { return SessionManager.Instance.Generate(); },
                DummyClientCount);

            Tick();

            while (true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}
