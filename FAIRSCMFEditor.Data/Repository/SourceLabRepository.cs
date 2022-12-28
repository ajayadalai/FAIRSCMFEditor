using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using FAIRSCMFEditor.Domain;

namespace FAIRSCMFEditor.Data
{
    public class SourceLabRepository : RepositoryBase
    {
        public override string TableName
        {
            get
            {
                return "SourceLab";
            }
        }

        public SourceLabRepository() : base(ConfigConstants.DBPath)
        {

        }
        
    }
}
