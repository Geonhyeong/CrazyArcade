using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server.Game
{
    internal struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick; // 실행 시간
        public IJob job;

        public int CompareTo(JobTimerElem other)
        {
            return other.execTick - execTick;
        }
    }

    public class JobTimer
    {
        private PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
        private object _lock = new object();

        public void Push(IJob job, int tickAfter = 0)
        {
            JobTimerElem jobElement;
            jobElement.execTick = System.Environment.TickCount + tickAfter;
            jobElement.job = job;

            lock (_lock)
            {
                _pq.Push(jobElement);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;

                JobTimerElem jobElement;
                lock (_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    jobElement = _pq.Peek();
                    if (jobElement.execTick > now)
                        break;

                    _pq.Pop();
                }

                jobElement.job.Execute();
            }
        }
    }
}