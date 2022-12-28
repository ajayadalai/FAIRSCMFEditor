using FAIRSCMFEditor.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Data
{
    public class DestinationORIRepository : RepositoryBase
    {
        public DestinationORIRepository() : base(ConfigConstants.DBPath) { }

        public override string TableName
        {
            get
            {
                return "DestinationORI";
            }
        }
    }
}
