using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public class Revision16Values
    {
        public string RunIdPath { get; set; }
        public List<string> SelectedXMLPath { get; set; }
        public string InstrumentName { get; set; }

        public string DestinationORI { get; set; }
        public string SourceORI { get; set; }
        public string CODISSpecimenCategory { get;  set; }
        public string SID { get;  set; }
        public string UCN { get;  set; }
        public string LivescanUniqueEventIdentifier { get;  set; }
        public string BookingAgencyConfigurableIdentifier { get;  set; }
        public string ArrestingAgencyConfigurableIdentifier { get;  set; }
        public string ArrestSubmissionDate { get;  set; }
        public string FingerprintCaptureDateTime { get;  set; }
        public string ArrestOffenseDescription { get;  set; }
        public string SpecimenComment { get;  set; }

        public int MessageId { get;  set; }

        public string SubmitBy { get;  set; }
        public string ReadingBy { get;  set; }

    }
}
