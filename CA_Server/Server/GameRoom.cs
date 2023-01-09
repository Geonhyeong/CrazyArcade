using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    internal class GameRoom : IJobQueue
    {
        private List<ClientSession> _sessions = new List<ClientSession>();
        private JobQueue _jobQueue = new JobQueue();
        private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            _pendingList.Add(segment);
            /*foreach (ClientSession s in _sessions)
                s.Send(segment);*/
        }
    }
}