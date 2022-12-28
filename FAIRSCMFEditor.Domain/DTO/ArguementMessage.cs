using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public class ArguementMessage:IViewEventArguments
    {
        public string Content { get; set; }

        public int Code { get; set; }

        public MessageType Type { get; set; }

    }

    public enum MessageType
    {
        Message,
        Confirm
    }
}
