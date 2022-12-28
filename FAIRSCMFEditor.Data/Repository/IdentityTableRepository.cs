using FAIRSCMFEditor.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Data.Repository
{
    public class IdentityTableRepository : RepositoryBase
    {
        public IdentityTableRepository() : base(ConfigConstants.DBPath) { }

        public override string TableName
        {
            get
            {
                return "IdentityTable";
            }
        }

        public int GetIdentityValue(string key)
        {
            int idValue = 0;
            using (var dataAccess = new DataAccess(DBPath))
            {
                var value = dataAccess.ExecuteScalar(string.Format("SELECT IDENTITYVALUE FROM {0} WHERE TABLEKEY = '{1}'", TableName, key));
                string command = string.Empty;
                if (value != DBNull.Value && value != null)
                {
                    idValue = Convert.ToInt32(value);
                    command = string.Format("UPDATE {0} SET IdentityValue = @value1 where TableKey='{1}'", TableName, key);
                }
                else
                {
                    command = string.Format("INSERT INTO {0}(TableKey,IdentityValue) Values('{1}',@value1)", TableName, key);
                }
                idValue++;
                var args = new Dictionary<string, object>
                {
                    { "@value1", idValue}
                };
                dataAccess.ExecuteSQL(command, args);
                return idValue;
            }
        }


    }
}
