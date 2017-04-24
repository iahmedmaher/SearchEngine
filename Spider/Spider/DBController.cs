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
        object PageContentTable;
        object LinkTableRead;
        object ImageTable;
        object StepsTable;

        System.Data.SQLite.SQLiteCommand GetLinkIdCommand;
        System.Data.SQLite.SQLiteCommand AddLinkCommand;
        System.Data.SQLite.SQLiteCommand AddPageVectorCommand;
        System.Data.SQLite.SQLiteCommand AddPageImagesCommand;
        System.Data.SQLite.SQLiteCommand AddPageContentCommand;
        System.Data.SQLite.SQLiteCommand AddPageStepsCommand;
        System.Data.SQLite.SQLiteCommand CheckLinkCountCommand;
        System.Data.SQLite.SQLiteCommand UpdateLinkInBoundCommand;
        System.Data.SQLite.SQLiteCommand UpdateLinkDateCommand;
        System.Data.SQLite.SQLiteCommand UpdateLinkTitleCommand;
        System.Data.SQLite.SQLiteCommand DeleteOldLinkVectorCommand;
        System.Data.SQLite.SQLiteCommand DeleteOldPageContentCommand;
        System.Data.SQLite.SQLiteCommand DeleteOldPageStepsCommand;

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

            GetLinkIdCommand = database.PrepareStatement(@"SELECT ID From URL WHERE URL=@link");
            AddLinkCommand = database.PrepareStatement(@"INSERT INTO URL(URL,Title,OutBound,TIMESTAMP) VALUES (@link,@title,@outbound,date('now'))");

            AddPageVectorCommand = database.PrepareStatement(@"INSERT INTO VECTOR(LID,Keyword,Rank) VALUES (@linkid, @keyword, @keywordrank)");

            AddPageImagesCommand = database.PrepareStatement(@"INSERT INTO Images(LID,ImageLink,ImageAlt) VALUES (@linkid, @imagelink, @imagealt)");

            AddPageStepsCommand = database.PrepareStatement(@"INSERT INTO StepsSuggestions(LID,Header,List) VALUES (@linkid,@header,@list)");

            AddPageContentCommand = database.PrepareStatement(@"INSERT INTO PageContent SELECT ID, @content FROM URL WHERE URL=@link ");
            CheckLinkCountCommand = database.PrepareStatement(@"SELECT COUNT(*) FROM URL WHERE URL=@link");
            UpdateLinkInBoundCommand = database.PrepareStatement(@"UPDATE URL SET InBound=InBound+1 WHERE URL=@link ");
            UpdateLinkDateCommand = database.PrepareStatement(@"UPDATE URL SET TIMESTAMP = date('now') WHERE URL = @link");
            UpdateLinkTitleCommand = database.PrepareStatement(@"UPDATE URL SET Title = @title WHERE URL=@link");
            DeleteOldLinkVectorCommand = database.PrepareStatement(@"DELETE FROM VECTOR WHERE LID = (SELECT ID FROM URL WHERE URL=@link)");
            DeleteOldPageContentCommand = database.PrepareStatement(@"DELETE FROM PageContent WHERE LID = (SELECT ID FROM URL WHERE URL=@link)");
            DeleteOldPageStepsCommand=database.PrepareStatement(@"DELETE FROM StepsSuggestions WHERE LID = (SELECT ID FROM URL WHERE URL=@link)");

            LinkTable = new object();
            ImageTable = new object();
            LinkTableRead = new object();
            VectorTable = new object();
            PageContentTable = new object();
            StepsTable = new object();

            Prepare();
        }

        private void Prepare()
        {
            string sql = "CREATE TABLE IF NOT EXISTS URL (ID INTEGER PRIMARY KEY AUTOINCREMENT,URL VARCHAR (300) NOT NULL UNIQUE, Title VARCHAR (150), InBound INT NOT NULL DEFAULT (1), OutBound INT NOT NULL DEFAULT (0), TIMESTAMP DATETIME NOT NULL);";

            database.ExecuteNonQuery(sql);

            sql = "CREATE VIRTUAL TABLE IF NOT EXISTS VECTOR USING fts3 (LID REFERENCES URL (ID) ON DELETE CASCADE ON UPDATE CASCADE,Keyword VARCHAR (20) NOT NULL, Rank REAL NOT NULL);";

            database.ExecuteNonQuery(sql);

            sql = "CREATE UNIQUE INDEX IF NOT EXISTS UniqueURL ON URL (URL);";

            database.ExecuteNonQuery(sql);

            sql = "CREATE VIRTUAL TABLE IF NOT EXISTS PageContent USING fts4 (LID REFERENCES URL (ID) ON DELETE CASCADE ON UPDATE CASCADE, Content TEXT);";

            database.ExecuteNonQuery(sql);

            sql = "CREATE VIRTUAL TABLE IF NOT EXISTS Images USING fts4 (LID REFERENCES URL (ID) ON DELETE CASCADE ON UPDATE CASCADE, ImageLink VARCHAR(150), ImageAlt TEXT);";

            database.ExecuteNonQuery(sql);

            sql = "CREATE VIRTUAL TABLE IF NOT EXISTS StepsSuggestions USING fts4 (LID REFERENCES URL (ID) ON DELETE CASCADE ON UPDATE CASCADE, Header TEXT, List VARCHAR(150));";

            database.ExecuteNonQuery(sql);

            sql = "PRAGMA synchronous = 0";

            database.ExecuteNonQuery(sql);

            Begin();

        }

        public void Begin()
        {
            database.ExecuteNonQuery("BEGIN TRANSACTION;");
        }

        public void Commit()
        {
            database.ExecuteNonQuery("COMMIT;");
        }
        public void AddLink(string link, string title, int OutBound)
        {
            lock (LinkTable)
            {
                AddLinkCommand.Parameters.AddWithValue("link", link);
                AddLinkCommand.Parameters.AddWithValue("title", title);
                AddLinkCommand.Parameters.AddWithValue("outbound", OutBound);
                AddLinkCommand.ExecuteNonQuery();
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

        public void UpdateLinkTitle(string link, string title)
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

        public void AddPageVector(string link, Dictionary<string, double> dictionary)
        {
            object LID;

            lock (LinkTableRead)
            {
                GetLinkIdCommand.Parameters.AddWithValue("link", link);
                LID = GetLinkIdCommand.ExecuteScalar();
            }

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
                    AddPageVectorCommand.Parameters.AddWithValue("linkid", LID);
                    AddPageVectorCommand.ExecuteNonQuery();
                }
            }
        }

        public void AddPageStepsList(string link, Dictionary<string,string> dictionary)
        {
            object LID;

            lock (LinkTableRead)
            {
                GetLinkIdCommand.Parameters.AddWithValue("link", link);
                LID = GetLinkIdCommand.ExecuteScalar();
            }

            foreach (var list in dictionary)
            {
                lock (StepsTable)
                {
                    if (Controller.OperationCancelled)
                        return;
                    
                    AddPageStepsCommand.Parameters.AddWithValue("header", list.Key);
                    AddPageStepsCommand.Parameters.AddWithValue("list", list.Value);
                    AddPageStepsCommand.Parameters.AddWithValue("linkid", LID);
                    AddPageStepsCommand.ExecuteNonQuery();
                }
            }
        }

        public void AddPageImages(string link, Dictionary<string, string> images)
        {
            object LID;

            lock (LinkTableRead)
            {
                GetLinkIdCommand.Parameters.AddWithValue("link", link);
                LID = GetLinkIdCommand.ExecuteScalar();
            }

            foreach (var Image in images)
            {
                lock (ImageTable)
                {
                    if (Controller.OperationCancelled)
                        return;
                    //@linkid, @imagelink, @imagealt @linkid, @imagelink, @imagealt
                    AddPageImagesCommand.Parameters.AddWithValue("linkid", LID);
                    AddPageImagesCommand.Parameters.AddWithValue("imagelink", Image.Key);
                    AddPageImagesCommand.Parameters.AddWithValue("imagealt", Image.Value);
                    AddPageImagesCommand.ExecuteNonQuery();
                }
            }
        }

        public void AddPageContent(string link, string content)
        {
            lock (PageContentTable)
            {
                AddPageContentCommand.Parameters.AddWithValue("link", link);
                AddPageContentCommand.Parameters.AddWithValue("content", content);
                AddPageContentCommand.ExecuteNonQuery();
            }
        }

        public void UpdatePageVector(string link, Dictionary<string, double> dictionary)
        {
            lock (VectorTable)
            {
                DeleteOldLinkVectorCommand.Parameters.AddWithValue("link", link);
                DeleteOldLinkVectorCommand.ExecuteNonQuery();
            }
            AddPageVector(link, dictionary);
        }

        public void UpdatePageStepsList(string link, Dictionary<string, string> dictionary)
        {
            lock (StepsTable)
            {
                DeleteOldPageStepsCommand.Parameters.AddWithValue("link", link);
                DeleteOldPageStepsCommand.ExecuteNonQuery();
            }
            AddPageStepsList(link, dictionary);
        }

        public void UpdatePageContent(string link, string content)
        {
            lock (PageContentTable)
            {
                DeleteOldPageContentCommand.Parameters.AddWithValue("link", link);
                DeleteOldLinkVectorCommand.ExecuteNonQuery();
            }
            AddPageContent(link, content);
        }

        public bool LinkExists(string link)
        {
            lock (LinkTable)
            {
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
            lock (LinkTable) lock (VectorTable) lock (PageContentTable) lock (ImageTable) lock(StepsTable) lock(LinkTableRead)
                    {
                        Commit();
                        database.SaveToDisk();
                        Begin();
                    }
        }
    }
}
