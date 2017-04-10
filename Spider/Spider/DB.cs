using System.Data;
using System.Data.SQLite;

namespace Spider
{
    class DB
    {
        SQLiteConnection connection;

        static DB singleton=null;

        public static DB GetInstance()
        {
            if (singleton == null)
            {
                singleton = new DB();
            }
            return singleton;
        }

        private DB()
        {
            connection = new SQLiteConnection("Data Source=:memory:; Version=3; UTF8Encoding=True;");
            connection.Open();
            string FileName = Properties.Settings.Default.PATH + Properties.Settings.Default.Indexer;
            using (SQLiteConnection sql = new SQLiteConnection("Data Source=" + FileName + "; Version=3; UTF8Encoding=True;"))
            {
                sql.Open();
                sql.BackupDatabase(connection, "main", "main", -1, null, -1);
                sql.Close();
            }
        }

        public int ExecuteNonQuery(string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            return command.ExecuteNonQuery();
        }

        public DataTable ExecuteReader(string query)
        {

            var command = connection.CreateCommand();
            command.CommandText = query;
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(reader);
                reader.Close();
                return dt;
            }
            else
            {
                reader.Close();
                return null;
            }

        }

        public object ExecuteScalar(string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            return command.ExecuteScalar();
        }

        public SQLiteCommand PrepareStatement(string query)
        {
            return new SQLiteCommand(query, connection);
        }

        public void SaveToDisk()
        {
            string FileName = Properties.Settings.Default.PATH + Properties.Settings.Default.Indexer;
            var output = new SQLiteConnection("Data Source=" + FileName + "; Version=3; UTF8Encoding=True;");
            output.Open();
            connection.BackupDatabase(output, "main", "main", -1, null, -1);
        }

        ~DB()
        {
            //SQLconnection.Close();
        }

    }
}
