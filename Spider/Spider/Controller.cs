using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Spider
{
    class Controller
    {
        ConcurrentQueue<string>[] Scheduled_links;
        DBController Database;
        GUI reporter;
        private ThreadManager TManager;
        private Task ProgressBackupTask;

        private LinkedList<ConcurrentQueue<string>> temp_list;
        static public bool OperationCancelled = false;

        public int MaxThreads
        {
            set
            {
                TManager.MaxThreads = value;
            }
        }

        public int QueueCount
        {
            get
            {
                if (Scheduled_links == null)
                    return temp_list.Count;

                int c = 0;
                foreach (var q in Scheduled_links)
                {
                    c += q.Count;
                }
                return c;
            }
        }

        public int ProcessedCount
        {
            get
            {
                return Database.GetCount();
            }
        }

        public Controller(GUI form)
        {
            TManager = new ThreadManager(form);
            ProgressBackupTask = new Task(PeriodicBackup);
            
            reporter = form;
            //?why 
            MaxThreads = Environment.ProcessorCount;

            Database = DBController.GetInstance();

            temp_list = null;

            HtmlParser.IntializeStopWords();
        }

        public bool CheckPreviousWork()
        {
            return File.Exists(Properties.Settings.Default.PATH + Properties.Settings.Default.QueueFile);
        }

        public void ResumeWork()
        {
            string FileName = Properties.Settings.Default.PATH + Properties.Settings.Default.QueueFile;
            Stream TestFileStream = File.OpenRead(FileName);
            BinaryFormatter deserializer = new BinaryFormatter();
            Scheduled_links = (ConcurrentQueue<String>[])deserializer.Deserialize(TestFileStream);
            TestFileStream.Close();
            StartProcess();
        }

        public void DiscardPreWork()
        {
            File.Delete(Properties.Settings.Default.PATH + Properties.Settings.Default.QueueFile);
        }

        public void StartProcess()
        {
            if (temp_list != null) 
                Scheduled_links = temp_list.ToArray();

            TManager.SetQueue(Scheduled_links);
            temp_list = null;
            TManager.StartProcess();
            if (!(ProgressBackupTask.Status == TaskStatus.Running))
            {
                ProgressBackupTask.Start();
            }
        }

        private void PeriodicBackup()
        {
            TimeSpan period = TimeSpan.FromMinutes(5);
            Thread.Sleep(period);

            while (!OperationCancelled)
            {
                SaveWork();
                Thread.Sleep(period);
            }
        }

        public void SaveWork()
        {
            string FileName = Properties.Settings.Default.PATH + Properties.Settings.Default.QueueFile;
            reporter.Invoke(reporter.ReportDBSaving);
            Database.SaveToDisk();

            int count = 0;

            if (Scheduled_links == null)
                return;

            foreach(var q in Scheduled_links)
            {
                count += q.Count;
            }

            if (count == 0)
                return;

            Stream TestFileStream = File.Create(FileName);
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(TestFileStream, Scheduled_links);
            TestFileStream.Close();
            reporter.Invoke(reporter.ReportDBSaved);
        }

        public bool Seed(string seed)
        {
            if (temp_list == null)
            {
                temp_list = new LinkedList<ConcurrentQueue<string>>();
            }

            if (!Database.LinkExists(seed) && RobotstxtParser.Approved(seed))
            {
                ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
                queue.Enqueue(seed);
                temp_list.AddLast(queue);
                reporter.Invoke(reporter.ReportQueued, seed + Environment.NewLine);
                return true;
            }
            return false;
        }

        public void AbortAll()
        {
            Controller.OperationCancelled = true;

            new Task(() =>
                {
                    TManager.WaitAll();
                    SaveWork();
                    reporter.Invoke(reporter.CancellationFinished);
                }).Start();
        }
    }
}
