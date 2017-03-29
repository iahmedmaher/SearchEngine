using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace Spider
{
    class Controller
    {
        ConcurrentQueue<string> Scheduled_links;
        DBController Database;
        GUI reporter;
        private ThreadManager TManager;
        private Task ProgressBackupTask;

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
                return Scheduled_links.Count;
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

            this.reporter = form;
            this.MaxThreads = Environment.ProcessorCount;

            Database = DBController.GetInstance();

            Scheduled_links = new ConcurrentQueue<string>();
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
            Scheduled_links = (ConcurrentQueue<String>)deserializer.Deserialize(TestFileStream);
            TestFileStream.Close();
            this.StartProcess();
        }

        public void DiscardPreWork()
        {
            File.Delete(Properties.Settings.Default.PATH + Properties.Settings.Default.QueueFile);
        }

        public void StartProcess()
        {
            TManager.SetQueue(Scheduled_links);
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

            if (Scheduled_links.Count == 0)
                return;

            Stream TestFileStream = File.Create(FileName);
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(TestFileStream, this.Scheduled_links);
            TestFileStream.Close();
            reporter.Invoke(reporter.ReportDBSaved);
        }

        public bool Seed(string seed)
        {
            if (!Database.LinkExists(seed) && RobotstxtParser.Approved(seed))
            {
                Scheduled_links.Enqueue(seed);
                reporter.Invoke(reporter.ReportQueued, seed + Environment.NewLine);
                return true;
            }
            return false;
        }

        //Is this step really necessary ? Practical runs showed only <1k out of 11k are duplicates
        private void RemoveQueueDuplicates()
        {
            ConcurrentQueue<string> temp = new ConcurrentQueue<string>();

            foreach (var link in Scheduled_links)
            {
                if (Database.LinkExists(link))
                    Database.LinkHit(link);
                else
                    temp.Enqueue(link);
            }

            Scheduled_links = temp;
        }

        public void AbortAll()
        {
            Controller.OperationCancelled = true;

            new Task(() =>
                {
                    TManager.WaitAll();
                    //RemoveQueueDuplicates();
                    SaveWork();
                    reporter.Invoke(reporter.CancellationFinished);
                }).Start();
        }
    }
}
