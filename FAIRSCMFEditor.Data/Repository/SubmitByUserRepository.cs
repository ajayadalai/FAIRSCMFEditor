using FAIRSCMFEditor.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Data.Repository
{
    public class SubmitByUserRepository:RepositoryBase
    {
        public SubmitByUserRepository() : base(ConfigConstants.DBPath) { }

        public override string TableName
        {
            get
            {
                return "SubmitByUser";
            }
        }
    }
}
