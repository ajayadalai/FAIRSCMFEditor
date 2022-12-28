using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public abstract class FileGeneratorFactory
    {
        public abstract FileGenerator GetFileGenerator();
    }
}
