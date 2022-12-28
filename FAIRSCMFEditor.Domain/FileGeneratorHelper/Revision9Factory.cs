using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public class Revision9Factory : FileGeneratorFactory
    {
        public string RunIdPath { get; private set; }
        public List<string> SelectedCMFFiles { get; private set; }
        public string InstrumentName { get; private set; }
        public string SourceLab { get; private set; }
        public string DestinationORI { get; private set; }
        public string SpecimenCategory { get; private set; }
        public string ReadingBy { get; private set; }
        public int MessageId { get; private set; }

        public Revision9Factory(string runIdPath, List<string> selectedFiles, string instrumentName, string sourceLab, string destinationORI, string specimenCategory, string readingByUser, int messageId)
        {
            RunIdPath = runIdPath;
            SelectedCMFFiles = selectedFiles;
            InstrumentName = instrumentName;
            SourceLab = sourceLab;
            DestinationORI = destinationORI;
            SpecimenCategory = specimenCategory;
            ReadingBy = readingByUser;
            MessageId = messageId;
        }

        public override FileGenerator GetFileGenerator()
        {
            return new Revision9FileGenerator(RunIdPath, SelectedCMFFiles, InstrumentName, SourceLab, DestinationORI, SpecimenCategory, ReadingBy, MessageId);
        }
    }
}
