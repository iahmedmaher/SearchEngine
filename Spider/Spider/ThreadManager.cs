using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Spider
{
    class ThreadManager
    {
        ConcurrentQueue<string> Scheduled_links;
        DBController Database;
        object ClosingLock;
        GUI reporter;
        public int MaxThreads { set; get; }
        private int ThreadCount;
        private bool busy;
        ThreadWorker worker;

        public ThreadManager(GUI RP)
        {
            this.Database = DBController.GetInstance();
            this.ClosingLock = new object();
            this.reporter = RP;
            this.ThreadCount = 0;
            this.busy = false;
        }

        private void RunThread(object obj)
        {
            Interlocked.Increment(ref ThreadCount);
            worker.ThreadProc(obj);
            Interlocked.Decrement(ref ThreadCount);
            
            if (ThreadCount == 0)
                lock (ClosingLock) { Monitor.Pulse(ClosingLock); }
        }

        public void SetQueue(ConcurrentQueue<string> SL)
        {
            this.Scheduled_links = SL;
            this.worker = new ThreadWorker(SL, this.reporter);
        }

        public void StartProcess()
        {
            new Task(Manage).Start();
        }

        private void Manage()
        {
            busy = true;
            string url;

            do
            {
                if (ThreadCount < MaxThreads && Scheduled_links.TryDequeue(out url))
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(RunThread), url);
                }

                if (Controller.OperationCancelled)
                {
                    busy = false;
                    return;
                }

                Thread.Sleep(500);

            } while (Scheduled_links.Count > 0 || ThreadCount > 0);

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
