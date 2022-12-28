using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FAIRSCMFEditor.Domain
{
    public class Revision13Factory : FileGeneratorFactory
    {
        public string RunIdPath { get; private set; }
        public string XmlFile { get; private set; }
        public string InstrumentName { get; private set; }
        public string SourceLab { get; private set; }
        public string SubmitBy { get; private set; }
        public string DestinationORI { get; private set; }
        public string SpecimenCategory { get; private set; }
        public string ReadingBy { get; private set; }
        public XmlDocument XmlDoc { get; private set; }

        public string SpecimentSourceID { get; private set; }

        public string SpecimenPartial { get; private set; }

        public List<string> SelectedXmlFiles { get; private set; }

        public Revision13Factory(string runIdPath, List<string> SelectedFiles, string instrumentName, string sourceLab, string destinationORI, string submitBy, string specimenCategory, string readingByUser,
           string specimentSourceId, string specimenPartial, XmlDocument xdocument)
        {
            RunIdPath = runIdPath;
          //  XmlFile = xmlFile;
            SelectedXmlFiles = SelectedFiles;
            InstrumentName = instrumentName;
            SourceLab = sourceLab;
            DestinationORI = destinationORI;
            SpecimenCategory = specimenCategory;
            ReadingBy = readingByUser;
            SubmitBy = submitBy;
            XmlDoc = xdocument;
            SpecimentSourceID = specimentSourceId;
            SpecimenPartial = specimenPartial;
        }

        public override FileGenerator GetFileGenerator()
        {
            return new Revision13FileGenerator(RunIdPath, InstrumentName, SourceLab, DestinationORI, SubmitBy, SpecimenCategory, ReadingBy, SpecimentSourceID, SpecimenPartial, XmlFile, XmlDoc,SelectedXmlFiles);
        }
    }
}
