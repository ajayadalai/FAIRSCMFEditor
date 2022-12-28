using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public class EventArguements<T>
    {
        public dynamic EventData { get; private set; }

        public EventArguements(T dataum)
        {
            EventData = dataum;
        }
        
        public bool IsMessage
        {
            get { return EventData is ArguementMessage; }
        }
    }
}
