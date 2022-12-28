using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public abstract class ProfileReader
    {
        public abstract Result ReadProfiles(Stream stream);

        public class Result
        {
            public Result()
            {
                Profiles = new List<Profile>();
                Recognized = false;
                Error = false;
                Message = string.Empty;
            }

            public List<Profile> Profiles { get; set; }
            public bool Recognized { get; set; }
            public bool Error { get; set; }
            public string Message { get; set; }

            public static readonly Result None = new Result();
        }
    }
}
