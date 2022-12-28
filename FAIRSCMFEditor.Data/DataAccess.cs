using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;

namespace FAIRSCMFEditor.Data
{
    public class DataAccess:IDisposable
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private static SQLiteConnection dbConnection = null;

        private static string dbPath;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="platformDetails"></param>
        public DataAccess(string datasource)
        {
            dbPath = datasource;
        }

        private static void CreateDBIfNotExists()
        {
            var directoryName = System.IO.Path.GetDirectoryName(dbPath);
            if(!System.IO.Directory.Exists(directoryName))
            {
                System.IO.Directory.CreateDirectory(directoryName);
            }
            if(!System.IO.File.Exists(dbPath))
            {
                //System.IO.File.Create(dbPath);
                SQLiteConnection.CreateFile(dbPath);
                
            }
            CreateSourceLabTable();
            CreateDestinationORITable();
            CreateReadingByUserIdTable();
            CreateSpecimenCategoryTable();
            CreateSubmitByUserIdTable();
            CreateIdentityTable();
            CreateAppVersionTable();
        }

        private static void CreateIdentityTable()
        {
            if (!TableExists("IdentityTable"))
            {
                string sqlCommand = "CREATE TABLE IdentityTable (ID INTEGER PRIMARY KEY AUTOINCREMENT,TableKey Text, IdentityValue INTEGER)";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void CreateSpecimenCategoryTable()
        {
            if(!TableExists("SpecimenCategory"))
            {
                string sqlCommand = "CREATE TABLE SpecimenCategory (ID INTEGER PRIMARY KEY AUTOINCREMENT,ValueList Text)";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void CreateReadingByUserIdTable()
        {
            if (!TableExists("ReadingByUser"))
            {
                string sqlCommand = "CREATE TABLE ReadingByUser (ID INTEGER PRIMARY KEY AUTOINCREMENT,ValueList Text)";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void CreateSubmitByUserIdTable()
        {
            if (!TableExists("SubmitByUser"))
            {
                string sqlCommand = "CREATE TABLE SubmitByUser (ID INTEGER PRIMARY KEY AUTOINCREMENT,ValueList Text)";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void CreateDestinationORITable()
        {
            if (!TableExists("DestinationORI"))
            {
                string sqlCommand = "CREATE TABLE DestinationORI (ID INTEGER PRIMARY KEY AUTOINCREMENT,ValueList Text)";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void CreateSourceLabTable()
        {
            if (!TableExists("SourceLab"))
            {
                string sqlCommand = "CREATE TABLE SourceLab (ID INTEGER PRIMARY KEY AUTOINCREMENT,ValueList Text)";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void CreateAppVersionTable()
        {
            if (!TableExists("AppVersion"))
            {
                string sqlCommand = "CREATE TABLE AppVersion (ID INTEGER PRIMARY KEY AUTOINCREMENT,Version Text)";
                ExecuteSQLCommand(sqlCommand);
            }
        }


        public void DropTables()
        {
            DropIdentityTable();
            DropSpecimenCategoryTable();
            DropReadingByUserIdTable();
            DropSubmitByUserIdTable();
            DropDestinationORITable();
            DropSourceLabTable();
            DropAppVersionTable();
            CreateDBIfNotExists();
        }

        private static void DropIdentityTable()
        {
            if (TableExists("IdentityTable"))
            {
                string sqlCommand = "DROP TABLE IdentityTable";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void DropSpecimenCategoryTable()
        {
            if (TableExists("SpecimenCategory"))
            {
                string sqlCommand = "DROP TABLE SpecimenCategory";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void DropReadingByUserIdTable()
        {
            if (TableExists("ReadingByUser"))
            {
                string sqlCommand = "DROP TABLE ReadingByUser";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void DropSubmitByUserIdTable()
        {
            if (TableExists("SubmitByUser"))
            {
                string sqlCommand = "DROP TABLE SubmitByUser";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void DropDestinationORITable()
        {
            if (TableExists("DestinationORI"))
            {
                string sqlCommand = "DROP TABLE DestinationORI";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void DropSourceLabTable()
        {
            if (TableExists("SourceLab"))
            {
                string sqlCommand = "DROP TABLE SourceLab";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        public void DropAppVersionTable()
        {
            if (TableExists("AppVersion"))
            {
                string sqlCommand = "DROP TABLE AppVersion";
                ExecuteSQLCommand(sqlCommand);
            }
        }

        private static void ExecuteSQLCommand(string commandText, Dictionary<string, object> parameters = null)
        {
            var command = new SQLiteCommand(commandText,DbConnection);
            command.Connection = DbConnection;
            DbConnection.Open();
            if (null != parameters && parameters.Count > 0)
            {
                foreach (var pair in parameters)
                {
                    command.Parameters.AddWithValue(pair.Key, pair.Value);
                }
            }
            command.ExecuteNonQuery();
            DbConnection.Close();
        }

        public static bool TableExists(string tableName)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE name='" + tableName + "'", DbConnection);
            DbConnection.Open();
            var result = command.ExecuteScalar();
            DbConnection.Close();
            return result != null && result.ToString() == tableName ? true : false;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>The connection.</value>
        private static SQLiteConnection DbConnection
        {
            get
            {
                if(dbConnection == null)
                {
                    dbConnection = new SQLiteConnection(string.Format("Data Source={0}", dbPath));
                    CreateDBIfNotExists();
                }
                return dbConnection;
                
            }
        }

        public void Dispose()
        {
            if (null != DbConnection)
            {
                if (DbConnection.State != ConnectionState.Closed)
                {
                    DbConnection.Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(SQLiteCommand command)
        {
            try
            {
                command.Connection = DbConnection;
                OpenConnection();
                var adapter = new SQLiteDataAdapter(command);
                var dt = new DataTable();
                adapter.Fill(dt);
                adapter.Dispose();
                CloseConnection();
                return dt;
            }
            catch (Exception ex)
            {
                var e = ex;
                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string commandText)
        {
            SQLiteCommand cmd = new SQLiteCommand(commandText);
            return ExecuteDataTable(cmd);
        }

        public int ExecuteSQL(string sql, Dictionary<string, object> parameters = null)
        {
            int numberOfRowsAffected;

            OpenConnection();
            var command = new SQLiteCommand(sql, DbConnection);
            if(null != parameters && parameters.Count > 0)
            {
                foreach (var pair in parameters)
                {
                    command.Parameters.AddWithValue(pair.Key, pair.Value);
                }
            }
            numberOfRowsAffected = command.ExecuteNonQuery();
            CloseConnection();
            return numberOfRowsAffected;
        }


        public object ExecuteScalar(string sql, Dictionary<string, object> parameters = null)
        {
            object retVal;

            OpenConnection();
            var command = new SQLiteCommand(sql, DbConnection);
            if (null != parameters && parameters.Count > 0)
            {
                foreach (var pair in parameters)
                {
                    command.Parameters.AddWithValue(pair.Key, pair.Value);
                }
            }
            retVal = command.ExecuteScalar();
            CloseConnection();
            command.Dispose();
            return retVal;
        }

        private void OpenConnection()
        {
            if (DbConnection.State != ConnectionState.Open)
            {
                DbConnection.Open();
            }
        }

        private void CloseConnection()
        {
            if (DbConnection.State != ConnectionState.Closed)
            {
                DbConnection.Close();
            }
        }
    }
}
