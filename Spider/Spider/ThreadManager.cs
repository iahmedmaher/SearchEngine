using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Spider
{
    class ThreadManager
    {
        ConcurrentQueue<string>[] Scheduled_links;
        DBController Database;
        object ClosingLock;
        GUI reporter;
        public int MaxThreads { set; get; }
        private int ThreadCount;
        private bool busy;
        private int CurrentQueue;
        ThreadWorker worker;

        public ThreadManager(GUI RP)
        {
            Database = DBController.GetInstance();
            ClosingLock = new object();
            reporter = RP;
            ThreadCount = 0;
            busy = false;
            CurrentQueue = 0;
        }

        private void RunThread(object obj)
        {
            Interlocked.Increment(ref ThreadCount);
            worker.ThreadProc(obj);
            Interlocked.Decrement(ref ThreadCount);
            
            if (ThreadCount == 0)
                lock (ClosingLock) { Monitor.Pulse(ClosingLock); }
        }

        public void SetQueue(ConcurrentQueue<string>[] SL)
        {
            Scheduled_links = SL;
            worker = new ThreadWorker(reporter);
        }

        public void StartProcess()
        {
            new Task(Manage).Start();
        }

        private bool QueueNotEmpty()
        {
            foreach (var q in Scheduled_links)
            {
                if (q.Count > 0)
                    return true;
            }
            return false;
        }

        private void Manage()
        {
            busy = true;
            string url;

            do
            {
                if (ThreadCount < MaxThreads && Scheduled_links[CurrentQueue].TryDequeue(out url))
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(RunThread), new ThreadParameter(Scheduled_links[CurrentQueue], url));
                }

                if (Controller.OperationCancelled)
                {
                    busy = false;
                    return;
                }
                CurrentQueue = (CurrentQueue + 1) % Scheduled_links.Length;
                Thread.Sleep(500);

            } while (QueueNotEmpty() || ThreadCount > 0);

            reporter.Invoke(reporter.ReportFinished);

            busy = false;
        }

        public void WaitAll()
        {
            if (busy)
            {
                lock (ClosingLock)
                {
                    Monitor.Wait(ClosingLock);
                }
            }
        }
    }
}
