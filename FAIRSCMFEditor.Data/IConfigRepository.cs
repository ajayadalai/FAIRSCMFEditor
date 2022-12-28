using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Data
{
    interface IConfigRepository
    {
        List<string> GetConfigValues();
        int AddConfigValues(List<string> valueList);
    }
}
