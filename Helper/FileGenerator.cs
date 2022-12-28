using FAIRSCMFEditor.Domain;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FAIRSCMFEditor.Helper
{
    public abstract class FileGenerator
    {
        public string RunIdPath { get; private set; }

        public string InstrumentName { get; private set; }

        public string SourceLab { get; private set; }

        public string DestinationORI { get; private set; }

        public string SpecimenCategory { get; private set; }

        public string ReadingBy { get; private set; }

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
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            desktopPath = desktopPath + "\\Fairs Data\\" + InstrumentName + "\\CMF Edited Files";
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

    public class Revision9FileGenerator : FileGenerator
    {
        public int MessageId { get; private set; }

        public List<string> SelectedCMFFiles { get; private set; }
        public Revision9FileGenerator(string runIdPath, List<string> selectedFiles, string instrumentName, string sourceLab, string destinationORI, string specimenCategory, string readingBy, int messageId)
            : base(runIdPath, instrumentName, sourceLab, destinationORI, specimenCategory, readingBy)
        {
            MessageId = messageId;
            SelectedCMFFiles = selectedFiles;
        }

        public override bool GenerateFile(out string message)
        {
            return CreateFileForRevision9(out message);
        }

        #region Revision9
        private bool CreateFileForRevision9(out string message)
        {
            bool isvalid = false;
            message = string.Empty;
            List<Profile> profileList = GetValidProfiles();
            if (profileList.Count == 0)
            {
                message = "The run id doesn't contain any valid sample.";
            }
            else
            {
                string content = PrepareFileContent(profileList);
                CreateTextFile(content);
                isvalid = true;
            }
            return isvalid;

        }

        private List<Profile> GetValidProfiles()
        {
            List<Profile> profileList = new List<Profile>();
            try
            {
                if (File.Exists(RunIdPath))
                {
                    using (ZipFile zip = new ZipFile(RunIdPath))
                    {
                        foreach (var file in SelectedCMFFiles)
                        {
                            var zipEntry = zip.Entries.FirstOrDefault(x => x.FileName == file);
                            if (zipEntry != null)
                            {
                                if (IsValidSample(zipEntry.FileName, zip))
                                {
                                    using (MemoryStream buffer = new MemoryStream())
                                    {
                                        zipEntry.Extract(buffer);
                                        buffer.Seek(0, 0);
                                        var reader = new CMFReader();
                                        var res = reader.ReadProfiles(buffer);
                                        if (res.Profiles.Count > 0)
                                        {
                                            profileList.Add(res.Profiles[0]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return profileList;
        }

        private bool IsValidSample(string fileName, ZipFile zip)
        {
            bool isValid = false;
            if (!fileName.ToLower().Contains("rfidmismatch"))
            {
                isValid = true;
                string _sampleInfoFileName = GetSampleFileInfoFileName(zip, fileName);
               
                if (!string.IsNullOrEmpty(_sampleInfoFileName))
                {
                    ZipEntry entry = zip.Entries.FirstOrDefault(x => x.FileName == _sampleInfoFileName);
                    if (entry != null)
                    {
                        using (var buffer = new MemoryStream())
                        {
                            entry.Extract(buffer);
                            buffer.Seek(0, 0);
                            using (var reader = new StreamReader(buffer))
                            {
                                string content = reader.ReadToEnd();
                                var lines = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                // Take out blank lines
                                var non_blank_lines = new List<string>();
                                foreach (var line in lines)
                                {
                                    string trim_line = line.Trim();
                                    if (string.IsNullOrEmpty(trim_line))
                                    {
                                        continue;
                                    }
                                    non_blank_lines.Add(trim_line);
                                }

                                // Do nothing if less than two lines
                                if (non_blank_lines.Count < 2)
                                {
                                    return false;
                                }

                                // Expecting just two lines
                                var header = non_blank_lines[0].Split(',').ToList();
                                var values = non_blank_lines[1].Split(',').ToList();

                                int index = header.IndexOf("IsValidSample");
                                if (index >= 0 && values.Count > index)
                                {
                                    string _value = values[index];
                                    if (_value.ToLower() == "true")
                                        isValid = true;
                                    else
                                        isValid = false;
                                }

                            }
                        } 
                    }

                }
            }
            return isValid;
        }


        const int FN_MIN_PARTS = 5;
        const int FN_RFID_REV_INDEX = 1; // Index from the last part
        static Regex laneRE_ = new Regex("L([0-9]+)");

        static Regex instRE_ = new Regex(@"(_[iI]([0-9]+)[.]\b[a-zA-Z]{3}\b$)|(_[iI]([0-9]+)_SampleInfo[.]\b[a-zA-Z]{3}\b$)", RegexOptions.IgnoreCase);
        private string GetSampleFileInfoFileName(ZipFile zip, string xmlfileName)
        {
            string _sampleInfoFileName = "";
            foreach (ZipEntry entry in zip)
            {
                var fn = entry.FileName;
                var inst_mc = instRE_.Matches(fn);
                if (inst_mc.Count > 0 && inst_mc[0].Groups.Count > 0)
                {
                    // Find the instrument designation in the file name
                    var grp = inst_mc[0].Groups[2];
                    if (fn.ToLower().EndsWith("sampleinfo.csv"))
                    {
                        grp = inst_mc[0].Groups[4];
                        //continue;
                    }
                    else
                    {
                        continue;
                    }

                    var start_of_fn_end = grp.Index + grp.Length;
                    if (start_of_fn_end >= fn.Length)
                    {
                        continue;
                    }
                    var end_of_fn = fn.Substring(start_of_fn_end);
                    if (!end_of_fn.ToLower().EndsWith(".csv"))
                    {
                        continue;
                    }

                    var base_fn = fn.Remove(fn.Length - end_of_fn.Length);
                    var fn_fields = base_fn.Split('_');
                    if (fn_fields.Length < FN_MIN_PARTS)
                    {
                        continue;
                    }

                    var rev_fn_fields = new List<string>(fn_fields.Reverse());
                    var lane_identifier = rev_fn_fields[FN_MIN_PARTS - 1];
                    var mc = laneRE_.Matches(lane_identifier);
                    if (mc.Count > 0 && mc[0].Groups.Count > 1)
                    {
                        // We match something like 'L3' - extract the '3'
                        //   var lane = int.Parse(mc[0].Groups[1].Value);
                        var sample_id = string.Join("_", fn_fields, 0, fn_fields.Length - FN_MIN_PARTS);
                        //    var rfid = rev_fn_fields[FN_RFID_REV_INDEX];
                        var sampleFileName = sample_id + "_" + lane_identifier;
                        if (xmlfileName.StartsWith(sampleFileName))
                        {
                            _sampleInfoFileName= fn;
                            break;
                        }

                    }
                }
            }


            return _sampleInfoFileName;
        }

        private void CreateTextFile(string content)
        {
            string EditedFilePath = GetFilePath();
            string DesktopEditedPath = GetDesktopPath();

            if (!Directory.Exists(EditedFilePath))
            {
                Directory.CreateDirectory(EditedFilePath);
            }
            if (!Directory.Exists(DesktopEditedPath))
            {
                Directory.CreateDirectory(DesktopEditedPath);
            }
            var currTimestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string txtfileName = string.Format("{0}_{1}.txt", GetEditedFileName(EditedFilePath), currTimestamp);
            string desktoptxtfileName = string.Format("{0}_{1}.txt", GetEditedFileName(DesktopEditedPath), currTimestamp);

            using (StreamWriter sw = File.CreateText(txtfileName))
            {
                sw.WriteLine(content);

            }
            using (StreamWriter sw = File.CreateText(desktoptxtfileName))
            {
                sw.WriteLine(content);

            }

        }

        private string PrepareFileContent(List<Profile> profileList)
        {
            StringBuilder sbContent = new StringBuilder();
            PrepareFileHeader(sbContent, profileList.Count);

            #region Profiles
            foreach (var profile in profileList)
            {
                PrepareProfileHeader(sbContent, profile);
                #region Locus
                foreach (var locus in profile.LociCalls)
                {
                    PrepareLocusHeader(sbContent, locus, profile);
                    PrepareAlleleValues(sbContent, locus.Value);
                }
                #endregion
            }
            #endregion
            return sbContent.ToString();
        }

        private void PrepareFileHeader(StringBuilder sbContent, int count)
        {
            sbContent.AppendLine(Revision9Constants.CMF_Header);
            sbContent.AppendLine(MessageId.ToString());
            sbContent.AppendLine(Revision9Constants.CMF_Message_Type);
            sbContent.AppendLine(SourceLab);
            sbContent.AppendLine(DestinationORI);
            sbContent.AppendLine(DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
            sbContent.AppendLine(Revision9Constants.Imaging_System_Org_Company);
            sbContent.AppendLine(Revision9Constants.Imaging_System_Utilized);
            sbContent.AppendLine(count.ToString()); ;
        }

        private void PrepareProfileHeader(StringBuilder sbContent, Profile profile)
        {
            sbContent.AppendLine(Revision9Constants.CMF_Packet_Type);
            sbContent.AppendLine(Revision9Constants.CMF_Packet_Version);
            sbContent.AppendLine(Revision9Constants.CODIS_Technology_Used_For_Analysis);
            sbContent.AppendLine(profile.CaseId);
            sbContent.AppendLine(Revision9Constants.CODIS_Sample_ID);
            sbContent.AppendLine(SpecimenCategory);
            sbContent.AppendLine(Revision9Constants.CODIS_Tissue_Type);
            sbContent.AppendLine(Revision9Constants.CODIS_Tissue_Form);
            sbContent.AppendLine(Revision9Constants.CODIS_Population_Group);
            sbContent.AppendLine(profile.LociCalls.Count.ToString());
        }

        private void PrepareLocusHeader(StringBuilder sbContent, KeyValuePair<string, HashSet<string>> locus, Profile profile)
        {
            sbContent.AppendLine(locus.Key);
            sbContent.AppendLine(Revision9Constants.Number_Of_Readings);
            sbContent.AppendLine(ReadingBy);
            sbContent.AppendLine(profile.Timestamp.ToString("dd-MMM-yyyy"));
            sbContent.AppendLine(profile.Timestamp.ToString("HH:mm:ss"));
            sbContent.AppendLine(locus.Value.Count().ToString());
        }

        private void PrepareAlleleValues(StringBuilder sbContent, HashSet<string> values)
        {
            foreach (var val in values)
            {
                sbContent.AppendLine(val);
            }
        }

        //private string GetMessageID()
        //{
        //    int MessageID = vm.GetMessageIDForRevision9();
        //    return MessageID.ToString();
        //}

        //private string GetSourceLab()
        //{
        //    if (cmbSourceLab.SelectedIndex > 0)
        //    {
        //        return cmbSourceLab.SelectedValue.ToString();
        //    }
        //    else
        //    {
        //        return txtSourceLab.Text;
        //    }
        //}

        //private string GetDestinationORI()
        //{
        //    if (cmbDestinationORI.SelectedIndex > 0)
        //    {
        //        return cmbDestinationORI.SelectedValue.ToString();
        //    }
        //    else
        //    {
        //        return txtDestinationORI.Text;
        //    }
        //}

        //private string GetSpecimentCategory()
        //{
        //    if (cmbSpecimenCat.SelectedIndex > 0)
        //    {
        //        return cmbSpecimenCat.SelectedValue.ToString();
        //    }
        //    else
        //    {
        //        return txtDestinationORI.Text;
        //    }
        //}

        //private string GetReadingByUser()
        //{
        //    if (cmbReadingBy.SelectedIndex > 0)
        //    {
        //        return cmbReadingBy.SelectedValue.ToString();
        //    }
        //    else
        //    {
        //        return txtReadingBy.Text;
        //    }
        //}
        #endregion
    }

    public class Revision13FileGenerator : FileGenerator
    {
        public string SubmitBy { get; private set; }
        XmlNamespaceManager nameSpaceManager = null;
        string namespaceName = "ns";
        string xmlFileName = string.Empty;
        XmlDocument xdoc;

        public Revision13FileGenerator(string runIdPath, string instrumentName, string sourceLab, string destinationORI, string submitBy, string specimenCategory, string readingBy, string xmlFile, XmlDocument xmlDocument)
            : base(runIdPath, instrumentName, sourceLab, destinationORI, specimenCategory, readingBy)
        {
            xmlFileName = xmlFile;
            SubmitBy = submitBy;
            xdoc = xmlDocument;
            createNameSpaceManager(xdoc);
        }

        public override bool GenerateFile(out string message)
        {
            message = string.Empty;
            bool retVal = false;
            CreateFileForRevision13(out retVal);
            return retVal;
        }

        private void CreateFileForRevision13(out bool isSucceed)
        {
            WriteValues(out isSucceed);

            xmlFileName = System.IO.Path.GetFileNameWithoutExtension(xmlFileName) + "_Final" + System.IO.Path.GetExtension(xmlFileName);

            string EditedFilePath = GetFilePath();
            string DesktopEditedPath = GetDesktopPath();

            if (!Directory.Exists(EditedFilePath))
            {
                Directory.CreateDirectory(EditedFilePath);
            }
            if (!Directory.Exists(DesktopEditedPath))
            {
                Directory.CreateDirectory(DesktopEditedPath);
            }
            string zipfileName = GetEditedFileName(EditedFilePath);
            string desktopZipfileName = GetEditedFileName(DesktopEditedPath);
            // saving temp xml file so that we can delete it after adding to zip
            string xmlFilePath = EditedFilePath + "\\" + xmlFileName;
            string desktopxmlFilePath = DesktopEditedPath + "\\" + xmlFileName;
            xdoc.Save(xmlFilePath);
            xdoc.Save(desktopxmlFilePath);
            //  xdoc.Save(desktopxmlFilePath + "\\" + xmlFileName);
            if (File.Exists(zipfileName + ".zip") && File.Exists(desktopZipfileName + ".zip"))
            {
                using (ZipFile zip = ZipFile.Read(zipfileName + ".zip"))
                {
                    foreach (ZipEntry entry in zip.Entries)
                    {
                        if (entry.FileName.Equals(xmlFileName))
                        {
                            zip.RemoveEntry(xmlFileName);
                            break;
                        }
                    }

                    AddFilesToZip(zip, EditedFilePath, zipfileName);
                    zip.Save(System.IO.Path.ChangeExtension(zipfileName, ".zip"));
                    File.Delete(xmlFilePath);
                }

                using (ZipFile zip = ZipFile.Read(desktopZipfileName + ".zip"))
                {
                    foreach (ZipEntry entry in zip.Entries)
                    {
                        if (entry.FileName.Equals(xmlFileName))
                        {
                            zip.RemoveEntry(xmlFileName);
                            break;
                        }
                    }

                    AddFilesToZip(zip, DesktopEditedPath, desktopZipfileName);
                    zip.Save(System.IO.Path.ChangeExtension(desktopZipfileName, ".zip"));
                    File.Delete(desktopxmlFilePath);
                }


            }
            else
            {
                using (ZipFile zip = new ZipFile())
                {
                    AddFilesToZip(zip, EditedFilePath, zipfileName);
                    zip.Save(System.IO.Path.ChangeExtension(zipfileName, ".zip"));
                    File.Delete(xmlFilePath);
                }

                using (ZipFile zip = new ZipFile())
                {
                    AddFilesToZip(zip, DesktopEditedPath, desktopZipfileName);
                    zip.Save(System.IO.Path.ChangeExtension(desktopZipfileName, ".zip"));
                    File.Delete(desktopxmlFilePath);
                }
            }

        }

        private void AddFilesToZip(ZipFile zip, string EditedFilePath, string zipfileName)
        {
            try
            {
                var files = GetFilesFromPath(EditedFilePath);

                foreach (var f in files)
                {
                    if (f.ToLower().EndsWith(".xml"))
                    {
                        try
                        {
                            zip.AddFile(f,
                                System.IO.Path.GetDirectoryName("newDB").
                                Replace(zipfileName, string.Empty));
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private string[] GetFilesFromPath(string editedFilePath)
        {
            return Directory.GetFiles(editedFilePath, "*",
                             SearchOption.TopDirectoryOnly).
                             Where(f => System.IO.Path.GetExtension(f).
                                 ToLowerInvariant() != ".zip").ToArray();
        }

        private void WriteValues(out bool isSucceed)
        {
            isSucceed = true;
            try
            {
                NodeWrite("//ns:CODISImportFile/ns:DESTINATIONORI", Convert.ToString(DestinationORI));
                NodeWrite("//ns:CODISImportFile/ns:SOURCELAB", Convert.ToString(SourceLab));
                NodeWrite("//ns:CODISImportFile/ns:SUBMITBYUSERID", Convert.ToString(SubmitBy));
                NodeWrite("//ns:CODISImportFile/ns:SPECIMEN/ns:SPECIMENCATEGORY", Convert.ToString(SpecimenCategory));

                NodeWrite("//ns:CODISImportFile/ns:KIT", "ANDE FlexPlex");
                XmlNodeList userNodes = xdoc.SelectNodes("//ns:CODISImportFile/ns:SPECIMEN/ns:LOCUS", nameSpaceManager);
                foreach (XmlNode userNode in userNodes)
                {
                    XmlNode node = userNode.ChildNodes.Item(1);
                    if (node != null)
                    {
                        node.InnerText = Convert.ToString(ReadingBy);
                    }
                }

                XmlNode specimenNode = xdoc.SelectSingleNode("//ns:CODISImportFile/ns:SPECIMEN", nameSpaceManager);
                if (specimenNode != null && specimenNode.Attributes.Count > 0)
                {
                    XmlNode specimenIDNode = xdoc.SelectSingleNode("//ns:CODISImportFile/ns:SPECIMEN/ns:SPECIMENID", nameSpaceManager);

                    XmlAttribute caseIdAttribute = specimenNode.Attributes["CASEID"];
                    if (caseIdAttribute != null && !string.IsNullOrEmpty(caseIdAttribute.Value))
                    {
                        specimenIDNode.InnerText = caseIdAttribute.Value;
                        caseIdAttribute.Value = string.Empty;
                    }

                }
            }
            catch (Exception ex)
            {
                isSucceed = false;
            }

        }

        private string NodeResults(string ChildNodePath)
        {
            string nodeValue = string.Empty;
            return nodeValue = xdoc.SelectSingleNode(ChildNodePath, nameSpaceManager).InnerText;
        }

        private void NodeWrite(string ChildNodePath, string value)
        {

            try
            {
                if (!value.Equals("Select"))
                {
                    XmlNode xmlnd = xdoc.SelectSingleNode(ChildNodePath, nameSpaceManager);
                    xmlnd.InnerText = value;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void createNameSpaceManager(XmlDocument xmlDoc)
        {
            if (xmlDoc.ChildNodes[1].Attributes != null)
            {
                var xmlns = xmlDoc.ChildNodes[1].Attributes["xmlns"];

                if (xmlns != null)
                {
                    nameSpaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                    nameSpaceManager.AddNamespace(namespaceName, xmlns.Value);
                    nameSpaceManager.AddNamespace(namespaceName, "urn:CODISImportFile-schema");
                }
            }
        }
    }

    public class Revision16FileGenerator : FileGenerator
    {
        XmlNamespaceManager nameSpaceManager = null;
        string namespaceName = "ns";
        XmlDocument xdoc;
        public Revision16Values Revision16values { get; private set; }

        public Revision16FileGenerator(Revision16Values revision16Values)
            : base(revision16Values.RunIdPath, revision16Values.InstrumentName, revision16Values.SourceORI, revision16Values.DestinationORI, revision16Values.CODISSpecimenCategory, revision16Values.ReadingBy)
        {
            Revision16values = revision16Values;
            LoadDocument(Revision16values.SelectedXMLPath[0]);
        }

        private void createNameSpaceManager(XmlDocument xmlDoc)
        {
            if (xmlDoc.ChildNodes[1].Attributes != null)
            {
                var xmlns = xmlDoc.ChildNodes[1].Attributes["xmlns"];

                if (xmlns != null)
                {
                    nameSpaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                    nameSpaceManager.AddNamespace(namespaceName, xmlns.Value);
                    nameSpaceManager.AddNamespace(namespaceName, "urn:CODISImportFile-schema");
                }
            }
        }

        public override bool GenerateFile(out string message)
        {
            bool isvalid = false;
            message = string.Empty;
            isvalid = CreateFileForRevision16(out message);
            return isvalid;
        }

        private bool CreateFileForRevision16(out string message)
        {
            bool isvalid = false;
            message = string.Empty;
            string timestamp = string.Empty;

            string EditedFilePath = GetFilePath();
            string DesktopEditedPath = GetDesktopPath();

            timestamp = DateTime.Now.ToString("MMddyyyyHHmmss");

            var profiles = GetValidProfiles();

            if (profiles.Count > 0)
            {
                if (!Directory.Exists(EditedFilePath))
                {
                    Directory.CreateDirectory(EditedFilePath);
                }

                string txtfileName = GetEditedFileName(EditedFilePath) + "_Final_" + timestamp + ".xml";
                string desktoptxtfileName = GetEditedFileName(DesktopEditedPath) + "_Final_" + timestamp + ".xml";

                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                settings.NewLineOnAttributes = true;

                using (XmlWriter writer = XmlWriter.Create(txtfileName, settings))
                {

                    writer.WriteStartElement("CODISRapidImportFile", "urn:CODISRapidImportFile-schema");
                    CreateHeader(writer);
                    CreateDevice(writer);
                    foreach (var profile in profiles)
                    {
                        CreateSpecimenHeader(profile, writer);
                    }

                    writer.WriteEndElement();
                    writer.Flush();
                }
                CopyFile(txtfileName, desktoptxtfileName, DesktopEditedPath);

                isvalid = true;
            }
            else
            {
                message = "The run id doesn't contain any valid sample.";
            }
            return isvalid;
        }

        private void CreateHeader(XmlWriter writer)
        {
            writer.WriteStartElement("HEADER");
            writer.WriteElementString("MESSAGEVERSION", Revision16Constants.Message_Verison);
            writer.WriteElementString("MESSAGETYPE", Revision16Constants.Message_Type);
            writer.WriteElementString("MESSAGEID", Revision16values.MessageId.ToString());
            writer.WriteElementString("MESSAGEDATETIME", NodeResults("//ns:CODISImportFile/ns:SUBMITDATETIME"));
            writer.WriteElementString("MSGCREATORUSERID", NodeResults("//ns:CODISImportFile/ns:SUBMITBYUSERID"));
            writer.WriteElementString("DESTINATIONORI", this.DestinationORI);
            writer.WriteElementString("SOURCEORI", this.SourceLab);
            writer.WriteEndElement();
        }

        private void CreateDevice(XmlWriter writer)
        {
            writer.WriteStartElement("DEVICE");
            writer.WriteElementString("INSTRUMENTID", NodeResults("//ns:CODISImportFile/ns:DESTINATIONORI"));
            writer.WriteElementString("MANUFACTURER", Revision16Constants.Instrument_Manufacturer);
            writer.WriteElementString("MODEL", Revision16Constants.Instrument_Model);
            writer.WriteElementString("SOFTWAREVERSION", Revision16Constants.Instrument_Software);
            writer.WriteEndElement();
        }

        private void CreateSpecimenHeader(Profile profile, XmlWriter writer)
        {

            writer.WriteStartElement("SPECIMEN");
            writer.WriteElementString("SPECIMENID", profile.SpecimenId);// NodeResults("//ns:CODISImportFile/ns:SPECIMEN/ns:SPECIMENID"));
            writer.WriteElementString("SPECIMENCATEGORY", Revision16values.CODISSpecimenCategory);

            if (!string.IsNullOrEmpty(Revision16values.SID))
            {
                writer.WriteElementString("SID", Revision16values.SID);
            }

            if (!string.IsNullOrEmpty(Revision16values.UCN))
            {
                writer.WriteElementString("FBI_NUMBER_UCN", Revision16values.UCN);
            }

            writer.WriteElementString("UNIQUEEVENTID", Revision16values.LivescanUniqueEventIdentifier);

            if (!string.IsNullOrEmpty(Revision16values.BookingAgencyConfigurableIdentifier))
            {
                writer.WriteElementString("BOOKINGCUSTOMID", Revision16values.BookingAgencyConfigurableIdentifier);
            }

            if (!string.IsNullOrEmpty(Revision16values.ArrestingAgencyConfigurableIdentifier))
            {
                writer.WriteElementString("ARRESTINGCUSTOMID", Revision16values.ArrestingAgencyConfigurableIdentifier);
            }

            if (!string.IsNullOrEmpty(Revision16values.ArrestSubmissionDate))
            {
                writer.WriteElementString("ARRESTDATE", Revision16values.ArrestSubmissionDate);
            }

            writer.WriteElementString("FINGERPRINTDATE", Revision16values.FingerprintCaptureDateTime);
            writer.WriteElementString("ARRESTOFFENSECATEGORY", Revision16values.ArrestOffenseDescription);

            if (!string.IsNullOrEmpty(Revision16values.SpecimenComment))
            {
                writer.WriteElementString("SPECIMENCOMMENT", Revision16values.SpecimenComment);
            }

            CreateAllele(profile, writer);

            writer.WriteEndElement(); //SPECIMEN END


        }

        private void CreateAllele(Profile profile, XmlWriter writer)
        {
            foreach (var locus in profile.LociCalls)
            {
                CreateLocusElement(profile.BatchId, writer, locus);
            }
        }

        private void CreateLocusElement(string batchId, XmlWriter writer, KeyValuePair<string, HashSet<string>> locusDetails)
        {
            writer.WriteStartElement("LOCUS");
            writer.WriteElementString("LOCUSNAME", locusDetails.Key);
            writer.WriteElementString("KIT", Revision16Constants.Kit);
            writer.WriteElementString("BATCHID", batchId);

            foreach (var allele in locusDetails.Value)
            {
                writer.WriteStartElement("ALLELE");
                writer.WriteElementString("ALLELEVALUE", allele);
                writer.WriteEndElement();
            }
            writer.WriteEndElement(); // LOCUS END
        }

        private List<Profile> GetValidProfiles()
        {
            List<Profile> profileList = new List<Profile>();
            try
            {
                if (File.Exists(RunIdPath))
                {
                    using (ZipFile zip = new ZipFile(RunIdPath))
                    {
                        foreach (var file in Revision16values.SelectedXMLPath)
                        {
                            var zipEntry = zip.Entries.FirstOrDefault(x => x.FileName == file);
                            if (zipEntry != null)
                            {
                                if (IsValidSample(zipEntry.FileName, zip))
                                {
                                    using (MemoryStream buffer = new MemoryStream())
                                    {
                                        zipEntry.Extract(buffer);
                                        buffer.Seek(0, 0);
                                        var reader = new CMFReader();
                                        var res = reader.ReadProfiles(buffer);
                                        if (res.Profiles.Count > 0)
                                        {
                                            profileList.Add(res.Profiles[0]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return profileList;
        }

        private bool IsValidSample(string fileName, ZipFile zip)
        {
            bool isValid = false;
            if (!fileName.ToLower().Contains("rfidmismatch"))
            {
                isValid = true;
                string _sampleInfoFileName = GetSampleFileInfoFileName(zip, fileName);

                if (!string.IsNullOrEmpty(_sampleInfoFileName))
                {
                    ZipEntry entry = zip.Entries.FirstOrDefault(x => x.FileName == _sampleInfoFileName);
                    if (entry != null)
                    {
                        using (var buffer = new MemoryStream())
                        {
                            entry.Extract(buffer);
                            buffer.Seek(0, 0);
                            using (var reader = new StreamReader(buffer))
                            {
                                string content = reader.ReadToEnd();
                                var lines = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                // Take out blank lines
                                var non_blank_lines = new List<string>();
                                foreach (var line in lines)
                                {
                                    string trim_line = line.Trim();
                                    if (string.IsNullOrEmpty(trim_line))
                                    {
                                        continue;
                                    }
                                    non_blank_lines.Add(trim_line);
                                }

                                // Do nothing if less than two lines
                                if (non_blank_lines.Count < 2)
                                {
                                    return false;
                                }

                                // Expecting just two lines
                                var header = non_blank_lines[0].Split(',').ToList();
                                var values = non_blank_lines[1].Split(',').ToList();

                                int index = header.IndexOf("IsValidSample");
                                if (index >= 0 && values.Count > index)
                                {
                                    string _value = values[index];
                                    if (_value.ToLower() == "true")
                                        isValid = true;
                                    else
                                        isValid = false;
                                }

                            }
                        }
                    }

                }
            }
            return isValid;
        }


        const int FN_MIN_PARTS = 5;
        const int FN_RFID_REV_INDEX = 1; // Index from the last part
        static Regex laneRE_ = new Regex("L([0-9]+)");

        static Regex instRE_ = new Regex(@"(_[iI]([0-9]+)[.]\b[a-zA-Z]{3}\b$)|(_[iI]([0-9]+)_SampleInfo[.]\b[a-zA-Z]{3}\b$)", RegexOptions.IgnoreCase);
        private string GetSampleFileInfoFileName(ZipFile zip, string xmlfileName)
        {
            string _sampleInfoFileName = "";
            foreach (ZipEntry entry in zip)
            {
                var fn = entry.FileName;
                var inst_mc = instRE_.Matches(fn);
                if (inst_mc.Count > 0 && inst_mc[0].Groups.Count > 0)
                {
                    // Find the instrument designation in the file name
                    var grp = inst_mc[0].Groups[2];
                    if (fn.ToLower().EndsWith("sampleinfo.csv"))
                    {
                        grp = inst_mc[0].Groups[4];
                        //continue;
                    }
                    else
                    {
                        continue;
                    }

                    var start_of_fn_end = grp.Index + grp.Length;
                    if (start_of_fn_end >= fn.Length)
                    {
                        continue;
                    }
                    var end_of_fn = fn.Substring(start_of_fn_end);
                    if (!end_of_fn.ToLower().EndsWith(".csv"))
                    {
                        continue;
                    }

                    var base_fn = fn.Remove(fn.Length - end_of_fn.Length);
                    var fn_fields = base_fn.Split('_');
                    if (fn_fields.Length < FN_MIN_PARTS)
                    {
                        continue;
                    }

                    var rev_fn_fields = new List<string>(fn_fields.Reverse());
                    var lane_identifier = rev_fn_fields[FN_MIN_PARTS - 1];
                    var mc = laneRE_.Matches(lane_identifier);
                    if (mc.Count > 0 && mc[0].Groups.Count > 1)
                    {
                        // We match something like 'L3' - extract the '3'
                        //   var lane = int.Parse(mc[0].Groups[1].Value);
                        var sample_id = string.Join("_", fn_fields, 0, fn_fields.Length - FN_MIN_PARTS);
                        //    var rfid = rev_fn_fields[FN_RFID_REV_INDEX];
                        var sampleFileName = sample_id + "_" + lane_identifier;
                        if (xmlfileName.StartsWith(sampleFileName))
                        {
                            _sampleInfoFileName = fn;
                            break;
                        }

                    }
                }
            }


            return _sampleInfoFileName;
        }

        private string NodeResults(string ChildNodePath)
        {
            string nodeValue = string.Empty;
            return nodeValue = xdoc.SelectSingleNode(ChildNodePath, nameSpaceManager).InnerText;
        }

        private void LoadDocument(string SelectedFileName)
        {
            try
            {
                xdoc = new XmlDocument();
                ZipEntry entry;
                using (ZipFile zip = new ZipFile(RunIdPath))
                {
                    entry = zip.Entries.FirstOrDefault(x => x.FileName == SelectedFileName);
                }
                if (entry != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        entry.Extract(ms);
                        ms.Position = 0;
                        var sr = new StreamReader(ms);
                        var myStr = sr.ReadToEnd();
                        xdoc.LoadXml(myStr);
                        createNameSpaceManager(xdoc);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void CopyFile(string sourcePath, string targetPath, string DesktopEditedPath)
        {
            if (!Directory.Exists(DesktopEditedPath))
            {
                Directory.CreateDirectory(DesktopEditedPath);
            }

            System.IO.File.Copy(sourcePath, targetPath, true);
        }

    }

    public abstract class FileGeneratorFactory
    {
        public abstract FileGenerator GetFileGenerator();
    }

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

        public Revision13Factory(string runIdPath, string xmlFile, string instrumentName, string sourceLab, string destinationORI, string submitBy, string specimenCategory, string readingByUser, XmlDocument xdocument)
        {
            RunIdPath = runIdPath;
            XmlFile = xmlFile;
            InstrumentName = instrumentName;
            SourceLab = sourceLab;
            DestinationORI = destinationORI;
            SpecimenCategory = specimenCategory;
            ReadingBy = readingByUser;
            SubmitBy = submitBy;
            XmlDoc = xdocument;
        }

        public override FileGenerator GetFileGenerator()
        {
            return new Revision13FileGenerator(RunIdPath, InstrumentName, SourceLab, DestinationORI, SubmitBy, SpecimenCategory, ReadingBy, XmlFile, XmlDoc);
        }
    }

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
