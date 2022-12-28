using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FAIRSCMFEditor.Domain
{
    public class Revision16Factory : FileGeneratorFactory
    {
        public string RunIdPath { get; private set; }
        public string SelectedXMLPath { get; private set; }
        public string InstrumentName { get; private set; }

        public string DestinationORI { get; private set; }
        public string SourceORI { get; private set; }
        public string CODISSpecimenCategory { get; private set; }
        public string SID { get; private set; }
        public string UCN { get; private set; }
        public string LivescanUniqueEventIdentifier { get; private set; }
        public string BookingAgencyConfigurableIdentifier { get; private set; }
        public string ArrestingAgencyConfigurableIdentifier { get; private set; }
        public string ArrestSubmissionDate { get; private set; }
        public string FingerprintCaptureDateTime { get; private set; }
        public string ArrestOffenseDescription { get; private set; }
        public string SpecimenComment { get; private set; }

        public int MessageId { get; private set; }

        public string SubmitBy { get; private set; }
        public string ReadingBy { get; private set; }
        public XmlDocument XmlDoc { get; private set; }

        public Revision16Values revision16values { get; private set; }

        public Revision16Factory(Revision16Values values)
        {
            revision16values = values;
        }

        public override FileGenerator GetFileGenerator()
        {
            return new Revision16FileGenerator(revision16values);
        }
    }
}
