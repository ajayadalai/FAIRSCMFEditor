using FAIRSCMFEditor.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Data
{
    public abstract class RepositoryBase : IConfigRepository
    {
        public abstract string TableName { get; }
        public RepositoryBase(string path)
        {
            DBPath = path;
        }
        public string DBPath { get; private set; }

        public virtual int AddConfigValues(List<string> valueList)
        {
            using (var dataAccess = new DataAccess(DBPath))
            {
                var rowcount = Convert.ToInt32(dataAccess.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", TableName)));
                string command = string.Empty;
                if (rowcount > 0)
                {
                    command = string.Format("UPDATE {0} SET ValueList = @value1", TableName);
                }
                else
                {
                    command = string.Format("INSERT INTO {0}(ValueList) Values(@value1)", TableName);
                }
                var args = new Dictionary<string, object>
                {
                    { "@value1", valueList.ToJson()}
                };
               return dataAccess.ExecuteSQL(command, args);
            }
        }

        public virtual List<string> GetConfigValues()
        {
            List<string> retVal = null;
            DataTable table = null;
            using (var dataAccess = new DataAccess(DBPath))
            {
                table = dataAccess.ExecuteDataTable(string.Format("SELECT * FROM {0}", TableName));
            }
            if (table != null && table.Rows.Count > 0)
            {
                retVal = Convert.ToString(table.Rows[0]["ValueList"].ToString()).ToObjects<string>();
            }
            return retVal;
        }
    }
}
