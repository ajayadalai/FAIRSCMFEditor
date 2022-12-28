using FAIRSCMFEditor.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Data.Repository
{
    public class ReadingByUserRepository:RepositoryBase
    {
        public ReadingByUserRepository() : base(ConfigConstants.DBPath) { }

        public override string TableName
        {
            get
            {
                return "ReadingByUser";
            }
        }
    }
}
