using FAIRSCMFEditor.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Data.Repository
{
    public class VersionRepository : RepositoryBase
    {
        public VersionRepository() : base(ConfigConstants.DBPath) { }

        public override string TableName
        {
            get
            {
                return "AppVersion";
            }
        }

        public void CreateTable(string value)
        {
            using (var dataAccess = new DataAccess(ConfigConstants.DBPath))
            {
                if (DataAccess.TableExists("AppVersion"))
                {
                    string command = string.Empty;
                    command = string.Format("INSERT INTO {0}(Version) Values(@value1)", TableName);
                    var args = new Dictionary<string, object>
                            {
                                { "@value1", value }
                            };
                    dataAccess.ExecuteSQL(command, args);
                }
            }
        }

        public string GetVersion()
        {
            string retVal = null;

            using (var dataAccess = new DataAccess(DBPath))
            {
                if (DataAccess.TableExists(TableName))
                    retVal = Convert.ToString(dataAccess.ExecuteScalar(string.Format("SELECT Version FROM {0}", TableName)));
            }

            return retVal;
        }

        public void DropTables()
        {
            using (var dataAccess = new DataAccess(DBPath))
            {
                dataAccess.DropTables();
            }
        }
    }
}
