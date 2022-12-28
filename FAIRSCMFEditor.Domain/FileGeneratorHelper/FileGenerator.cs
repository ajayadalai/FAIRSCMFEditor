using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public abstract class FileGenerator
    {
        public string RunIdPath { get; private set; }

        public string InstrumentName { get; private set; }

        public string SourceLab { get; private set; }

        public string DestinationORI { get; private set; }

        public string SpecimenCategory { get; private set; }

        public string ReadingBy { get; private set; }

        private const string PublicDesktopPath = "C:\\Users\\Public\\Desktop";

        public FileGenerator(string runIdPath, string instrumentName, string sourceLab, string destinationORI, string specimenCategory, string readingBy)
        {
            RunIdPath = runIdPath;
            InstrumentName = instrumentName;
            SourceLab = sourceLab;
            DestinationORI = destinationORI;
            SpecimenCategory = specimenCategory;
            ReadingBy = readingBy;
        }

        public abstract bool GenerateFile(out string message);

        protected string GetFilePath()
        {
            string path = "";
            if (File.Exists(RunIdPath))
            {
                int lastIndex = RunIdPath.LastIndexOf('\\');
                path = lastIndex > -1 ? RunIdPath.Substring(0, lastIndex) : RunIdPath;

                path = path + "\\CMF Edited Files";
            }
            return path;
        }

        protected string GetDesktopPath()
        {
            string desktopPath = PublicDesktopPath; //Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string fairsdataLocked = "FAIRS Data.{2559a1f2-21d7-11d4-bdaf-00c04f60b9f0}";
            if(!Directory.Exists(Path.Combine(desktopPath, "FAIRS Data")))
            {
                if (!Directory.Exists(Path.Combine(desktopPath, fairsdataLocked)))
                {
                    desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                }
            }
            string path = Path.Combine(desktopPath, "FAIRS Data");
            if(Directory.Exists(path))
            {
                desktopPath = Path.Combine(path, InstrumentName , "CMF Edited Files");
            }
            else
            {
                desktopPath = Path.Combine(desktopPath, fairsdataLocked, InstrumentName, "CMF Edited Files");
            }
            //desktopPath = desktopPath + "\\Fairs Data\\" + InstrumentName + "\\CMF Edited Files";
            return desktopPath;
        }

        protected string GetEditedFileName(string EditedFilePath)
        {
            string fileName = "";
            fileName = System.IO.Path.GetFileName(RunIdPath);
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Contains("_decrypted.zip") ? fileName.Replace("_decrypted.zip", string.Empty) : fileName;
                fileName = EditedFilePath + "\\" + fileName;
            }
            return fileName;
        }
    }
}
