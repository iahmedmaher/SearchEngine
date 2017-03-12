using System;
using System.Collections.Generic;

namespace Spider
{
    class DBController
    {
        static DBController singleton = null;

        DB database;
        object LinkTable;
        object VectorTable;

        //Redundant but added for speed
        HashSet<string> AddedSet;

        System.Data.SQLite.SQLiteCommand AddLinkCommand;
        System.Data.SQLite.SQLiteCommand AddPageVectorCommand;
        System.Data.SQLite.SQLiteCommand CheckLinkCountCommand;
        System.Data.SQLite.SQLiteCommand UpdateLinkInBoundCommand;
        System.Data.SQLite.SQLiteCommand UpdateLinkDateCommand;
        System.Data.SQLite.SQLiteCommand UpdateLinkTitleCommand;
        System.Data.SQLite.SQLiteCommand DeleteOldLinkVector;

        public static DBController GetInstance()
        {
            if (singleton == null)
            {
                singleton = new DBController();
            }
            return singleton;
        }

        private DBController()
        {
            database = DB.GetInstance();
            AddLinkCommand = database.PrepareStatement(@"INSERT INTO URL(URL,Title,OutBound,TIMESTAMP) VALUES (@link,@title,@outbound,date('now'))");
            AddPageVectorCommand = database.PrepareStatement(@"INSERT INTO VECTOR SELECT ID, @keyword, @keywordrank FROM URL WHERE URL=@link ");
            CheckLinkCountCommand = database.PrepareStatement(@"SELECT COUNT(*) FROM URL WHERE URL=@link");
            UpdateLinkInBoundCommand = database.PrepareStatement(@"UPDATE URL SET InBound=InBound+1 WHERE URL=@link ");
            UpdateLinkDateCommand = database.PrepareStatement(@"UPDATE URL SET TIMESTAMP = date('now') WHERE URL = @link");
            UpdateLinkTitleCommand = database.PrepareStatement(@"UPDATE URL SET Title = @title WHERE URL=@link");
            DeleteOldLinkVector = database.PrepareStatement(@"DELETE FROM VECTOR WHERE LID = (SELECT ID FROM URL WHERE URL=@link)");

            LinkTable = new object();
            VectorTable = new object();
            AddedSet = new HashSet<string>();
            Prepare();
        }

        private void Prepare()
        {
            string sql = "CREATE TABLE IF NOT EXISTS URL (ID INTEGER PRIMARY KEY AUTOINCREMENT,URL VARCHAR (300) NOT NULL UNIQUE, Title VARCHAR (150), InBound INT NOT NULL DEFAULT (1), OutBound INT NOT NULL DEFAULT (0), TIMESTAMP DATETIME NOT NULL);";

            database.ExecuteNonQuery(sql);

            sql = "CREATE TABLE IF NOT EXISTS VECTOR (LID REFERENCES URL (ID) ON DELETE CASCADE ON UPDATE CASCADE,Keyword VARCHAR (20) NOT NULL, Rank INTEGER NOT NULL);";

            database.ExecuteNonQuery(sql);

            sql = "CREATE UNIQUE INDEX IF NOT EXISTS UniqueURL ON URL (URL);";
			
			database.ExecuteNonQuery(sql);
        }

        public void AddLink(string link, string title, int OutBound)
        {
            lock (LinkTable)
            {
                AddLinkCommand.Parameters.AddWithValue("link", link);
                AddLinkCommand.Parameters.AddWithValue("title", title);
                AddLinkCommand.Parameters.AddWithValue("outbound", OutBound);
                AddLinkCommand.ExecuteNonQuery();
                AddedSet.Add(link);
            }
        }

        public void UpdateLinkDate(string link)
        {
            lock (LinkTable)
            {
                UpdateLinkDateCommand.Parameters.AddWithValue("link", link);
                UpdateLinkDateCommand.ExecuteNonQuery();
            }
        }

        public void UpdateLinkTitle(string link,string title)
        {
            lock (LinkTable)
            {
                UpdateLinkTitleCommand.Parameters.AddWithValue("link", link);
                UpdateLinkTitleCommand.Parameters.AddWithValue("title", title);
                UpdateLinkTitleCommand.ExecuteNonQuery();
            }
        }

        public DateTime? GetLinkDate(string link)
        {
            lock (LinkTable)
            {
                string sql = "SELECT TIMESTAMP FROM URL WHERE URL='" + link + "'";

                var tbl = database.ExecuteReader(sql);

                if (tbl == null)
                    return null;

                return DateTime.Parse(tbl.Rows[0]["TIMESTAMP"].ToString());
            }
        }

        public void AddPageVector(string link, Dictionary<string, int> dictionary)
        {

            foreach (var word in dictionary)
            {
                lock (VectorTable)
                {
                    if (Controller.OperationCancelled)
                        return;
                    //sql = "INSERT INTO VECTOR SELECT ID,'" + word.Key + "'," + word.Value + " FROM URL WHERE URL='" + link + "';";
                    //database.ExecuteNonQuery(sql);
                    AddPageVectorCommand.Parameters.AddWithValue("keyword", word.Key);
                    AddPageVectorCommand.Parameters.AddWithValue("keywordrank", word.Value);
                    AddPageVectorCommand.Parameters.AddWithValue("link", link);
                    AddPageVectorCommand.ExecuteNonQuery();
                }
            }
        }

        public void UpdatePageVector(string link, Dictionary<string, int> dictionary)
        {
            lock (VectorTable)
            {
                DeleteOldLinkVector.Parameters.AddWithValue("link", link);
                DeleteOldLinkVector.ExecuteNonQuery();
            }
            AddPageVector(link, dictionary);
        }

        public bool LinkExists(string link)
        {
            lock (LinkTable)
            {
                if (AddedSet.Contains(link))
                    return true;

                CheckLinkCountCommand.Parameters.AddWithValue("link", link);
                object count = CheckLinkCountCommand.ExecuteScalar();

                if (Convert.ToInt32(count) == 0)
                    return false;

                return true;
            }
        }

        public int GetCount()
        {
            return Convert.ToInt32(database.ExecuteScalar("SELECT COUNT(*) FROM URL"));
        }

        public void LinkHit(string link)
        {
            lock (LinkTable)
            {
                UpdateLinkInBoundCommand.Parameters.AddWithValue("link", link);
                UpdateLinkInBoundCommand.ExecuteNonQuery();
            }
        }

        public void SaveToDisk()
        {
            database.SaveToDisk();
        }

    }
}
