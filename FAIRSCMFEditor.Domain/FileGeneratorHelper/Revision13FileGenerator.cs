using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace FAIRSCMFEditor.Domain
{
    public class Revision13FileGenerator : FileGenerator
    {
        public string SubmitBy { get; private set; }
        XmlNamespaceManager nameSpaceManager = null;
        string namespaceName = "ns";
        string xmlFileName = string.Empty;
        public List<string> SelectedXmlFiles { get; private set; }
        XmlDocument xdoc;
        string SpecimenSourceID { get; set; }
        string SpecimenPartial { get; set; }

        public Revision13FileGenerator(string runIdPath, string instrumentName, string sourceLab, string destinationORI, string submitBy, string specimenCategory, string readingBy, 
            string specimenSourceId, string specimenPartial, string xmlFile, XmlDocument xmlDocument, List<string> selectedFiles)
            : base(runIdPath, instrumentName, sourceLab, destinationORI, specimenCategory, readingBy)
        {
            xmlFileName = xmlFile;
            SubmitBy = submitBy;
            SpecimenSourceID = specimenSourceId;
            SpecimenPartial = specimenPartial;
            xdoc = xmlDocument;
            SelectedXmlFiles = selectedFiles;
            LoadDocument(SelectedXmlFiles[0]);
            //createNameSpaceManager(xdoc);
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


        public override bool GenerateFile(out string message)
        {
            message = string.Empty;
            bool retVal = false;
            CreateFileForRevision13(out retVal);
            return retVal;
        }

        private void CreateFileForRevision13(out bool isSucceed)
        {
            isSucceed = true;
            string timestamp = string.Empty;

            string EditedFilePath = GetFilePath();
            string DesktopEditedPath = GetDesktopPath();

            timestamp = DateTime.Now.ToString("MMddyyyyHHmmss");
            var profiles = GetValidProfiles();

            if (profiles != null && profiles.Count > 0)
            {
                if (!Directory.Exists(EditedFilePath))
                {
                    Directory.CreateDirectory(EditedFilePath);
                }

                string xmlFileName = GetEditedFileName(EditedFilePath) + "_Final_" + timestamp + ".xml";
                string desktoptxtfileName = GetEditedFileName(DesktopEditedPath) + "_Final_" + timestamp + ".xml";
                StringBuilder sb = new StringBuilder();
                StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(sb, Encoding.UTF8);
                var settings = new XmlWriterSettings();
              
                settings.Indent = true;
                settings.CloseOutput = true;
          

                using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
                {

                    writer.WriteStartElement("CODISImportFile", "urn:CODISImportFile-schema");
                    CreateTopElementes(writer);
                    foreach (var profile in profiles)
                    {
                        CreateSpecimen(profile, writer);
                    }
                    writer.WriteEndElement();
                    writer.Close();
                    writer.Flush();

                }
                string xml = sb.ToString(); //Encoding.UTF8.GetString(memoryStream.ToArray());
                File.WriteAllText(xmlFileName, xml, Encoding.ASCII);
                CopyFile(xmlFileName, desktoptxtfileName, DesktopEditedPath);
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
        private void CreateSpecimen(Profile profile, XmlWriter writer)
        {
            writer.WriteStartElement("SPECIMEN");
            string partial = SpecimenPartial; //GetNodeAttributeValue("//ns:CODISImportFile/ns:SPECIMEN", "PARTIAL");
            if (!string.IsNullOrEmpty(partial))
            {
                writer.WriteAttributeString("PARTIAL", partial);
            }
            string sourceID = SpecimenSourceID; //string sourceID = GetNodeAttributeValue("//ns:CODISImportFile/ns:SPECIMEN", "SOURCEID");
            if (!string.IsNullOrEmpty(sourceID))
            {
                writer.WriteAttributeString("SOURCEID", sourceID);
            }
            string caseID = GetNodeAttributeValue("//ns:CODISImportFile/ns:SPECIMEN", "CASEID");
            if (!string.IsNullOrEmpty(caseID))
            {
                writer.WriteAttributeString("CASEID", "");
            }
            

            //XmlNode specimenNode = xdoc.SelectSingleNode("//ns:CODISImportFile/ns:SPECIMEN", nameSpaceManager);
            //if (specimenNode != null && specimenNode.Attributes.Count > 0)
            //{
            //    XmlNode specimenIDNode = xdoc.SelectSingleNode("//ns:CODISImportFile/ns:SPECIMEN/ns:SPECIMENID", nameSpaceManager);

            //    XmlAttribute caseIdAttribute = specimenNode.Attributes["CASEID"];
            //    if (caseIdAttribute != null && !string.IsNullOrEmpty(caseIdAttribute.Value))
            //    {
            //        specimenIDNode.InnerText = caseIdAttribute.Value;
            //        caseIdAttribute.Value = string.Empty;
            //    }

            //}



            writer.WriteElementString("SPECIMENID", profile.SampleId);
            writer.WriteElementString("SPECIMENCATEGORY", this.SpecimenCategory);
            writer.WriteElementString("SPECIMENCOMMENT", NodeResults("//ns:CODISImportFile/ns:SPECIMEN/ns:SPECIMENCOMMENT"));
            foreach (var locus in profile.LociCalls)
            {
                CreateLocusElement(profile.BatchId, writer, locus);
            }
            writer.WriteEndElement();
        }

        private void CreateLocusElement(string batchId, XmlWriter writer, KeyValuePair<string, HashSet<string>> locusDetails)
        {
            writer.WriteStartElement("LOCUS");
            if (locusDetails.Key.ToLower() == "pentae")
            {
                writer.WriteElementString("LOCUSNAME", "Penta E");
            }
            else
            {
                writer.WriteElementString("LOCUSNAME", locusDetails.Key);
            }
            //< READINGBY > Kevin.Ellis </ READINGBY >       < READINGDATETIME > 2014 - 08 - 22T19: 56:00 </ READINGDATETIME >
            writer.WriteElementString("READINGBY", this.ReadingBy);
            writer.WriteElementString("READINGDATETIME", NodeResults("//ns:CODISImportFile/ns:SPECIMEN/ns:LOCUS/ns:READINGDATETIME"));

            foreach (var allele in locusDetails.Value)
            {
                writer.WriteStartElement("ALLELE");
                writer.WriteElementString("ALLELEVALUE", allele);
                writer.WriteEndElement();
            }
            writer.WriteEndElement(); // LOCUS END
        }

        private string GetNodeAttributeValue(string nodePath, string attributeName)
        {
            string nodeValue = string.Empty;

            return nodeValue = xdoc.SelectSingleNode(nodePath, nameSpaceManager).Attributes[attributeName] != null ?
                xdoc.SelectSingleNode(nodePath, nameSpaceManager).Attributes[attributeName].Value : "";
        }

        private void CreateTopElementes(XmlWriter writer)
        {
            writer.WriteElementString("HEADERVERSION", Revision13Constants.Header_Version);
            writer.WriteElementString("MESSAGETYPE", Revision13Constants.Message_Type);
            writer.WriteElementString("DESTINATIONORI", this.DestinationORI);
            writer.WriteElementString("SOURCELAB", this.SourceLab);
            writer.WriteElementString("SUBMITBYUSERID", this.SubmitBy);
            writer.WriteElementString("SUBMITDATETIME", NodeResults("//ns:CODISImportFile/ns:SUBMITDATETIME"));
            writer.WriteElementString("BATCHID", NodeResults("//ns:CODISImportFile/ns:BATCHID"));
            writer.WriteElementString("KIT", Revision13Constants.Kit);//NodeResults("//ns:CODISImportFile/ns:KIT")

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
                        foreach (var file in SelectedXmlFiles)
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



        //private void CreateFileForRevision13(out bool isSucceed)
        //{
        //    WriteValues(out isSucceed);

        //    xmlFileName = System.IO.Path.GetFileNameWithoutExtension(xmlFileName) + "_Final" + System.IO.Path.GetExtension(xmlFileName);

        //    string EditedFilePath = GetFilePath();
        //    string DesktopEditedPath = GetDesktopPath();

        //    if (!Directory.Exists(EditedFilePath))
        //    {
        //        Directory.CreateDirectory(EditedFilePath);
        //    }
        //    if (!Directory.Exists(DesktopEditedPath))
        //    {
        //        Directory.CreateDirectory(DesktopEditedPath);
        //    }
        //    string zipfileName = GetEditedFileName(EditedFilePath);
        //    string desktopZipfileName = GetEditedFileName(DesktopEditedPath);
        //    // saving temp xml file so that we can delete it after adding to zip
        //    string xmlFilePath = EditedFilePath + "\\" + xmlFileName;
        //    string desktopxmlFilePath = DesktopEditedPath + "\\" + xmlFileName;
        //    xdoc.Save(xmlFilePath);
        //    xdoc.Save(desktopxmlFilePath);
        //    //  xdoc.Save(desktopxmlFilePath + "\\" + xmlFileName);
        //    if (File.Exists(zipfileName + ".zip") && File.Exists(desktopZipfileName + ".zip"))
        //    {
        //        using (ZipFile zip = ZipFile.Read(zipfileName + ".zip"))
        //        {
        //            foreach (ZipEntry entry in zip.Entries)
        //            {
        //                if (entry.FileName.Equals(xmlFileName))
        //                {
        //                    zip.RemoveEntry(xmlFileName);
        //                    break;
        //                }
        //            }

        //            AddFilesToZip(zip, EditedFilePath, zipfileName);
        //            zip.Save(System.IO.Path.ChangeExtension(zipfileName, ".zip"));
        //            File.Delete(xmlFilePath);
        //        }

        //        using (ZipFile zip = ZipFile.Read(desktopZipfileName + ".zip"))
        //        {
        //            foreach (ZipEntry entry in zip.Entries)
        //            {
        //                if (entry.FileName.Equals(xmlFileName))
        //                {
        //                    zip.RemoveEntry(xmlFileName);
        //                    break;
        //                }
        //            }

        //            AddFilesToZip(zip, DesktopEditedPath, desktopZipfileName);
        //            zip.Save(System.IO.Path.ChangeExtension(desktopZipfileName, ".zip"));
        //            File.Delete(desktopxmlFilePath);
        //        }


        //    }
        //    else
        //    {
        //        using (ZipFile zip = new ZipFile())
        //        {
        //            AddFilesToZip(zip, EditedFilePath, zipfileName);
        //            zip.Save(System.IO.Path.ChangeExtension(zipfileName, ".zip"));
        //            File.Delete(xmlFilePath);
        //        }

        //        using (ZipFile zip = new ZipFile())
        //        {
        //            AddFilesToZip(zip, DesktopEditedPath, desktopZipfileName);
        //            zip.Save(System.IO.Path.ChangeExtension(desktopZipfileName, ".zip"));
        //            File.Delete(desktopxmlFilePath);
        //        }
        //    }

        //}

        //private void AddFilesToZip(ZipFile zip, string EditedFilePath, string zipfileName)
        //{
        //    try
        //    {
        //        var files = GetFilesFromPath(EditedFilePath);

        //        foreach (var f in files)
        //        {
        //            if (f.ToLower().EndsWith(".xml"))
        //            {
        //                try
        //                {
        //                    zip.AddFile(f,
        //                        System.IO.Path.GetDirectoryName("newDB").
        //                        Replace(zipfileName, string.Empty));
        //                }
        //                catch { }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        //private string[] GetFilesFromPath(string editedFilePath)
        //{
        //    return Directory.GetFiles(editedFilePath, "*",
        //                     SearchOption.TopDirectoryOnly).
        //                     Where(f => System.IO.Path.GetExtension(f).
        //                         ToLowerInvariant() != ".zip").ToArray();
        //}

        //private void WriteValues(out bool isSucceed)
        //{
        //    isSucceed = true;
        //    try
        //    {
        //        NodeWrite("//ns:CODISImportFile/ns:DESTINATIONORI", Convert.ToString(DestinationORI));
        //        NodeWrite("//ns:CODISImportFile/ns:SOURCELAB", Convert.ToString(SourceLab));
        //        NodeWrite("//ns:CODISImportFile/ns:SUBMITBYUSERID", Convert.ToString(SubmitBy));
        //        NodeWrite("//ns:CODISImportFile/ns:SPECIMEN/ns:SPECIMENCATEGORY", Convert.ToString(SpecimenCategory));

        //        NodeWrite("//ns:CODISImportFile/ns:KIT", "ANDE FlexPlex");
        //        XmlNodeList userNodes = xdoc.SelectNodes("//ns:CODISImportFile/ns:SPECIMEN/ns:LOCUS", nameSpaceManager);
        //        foreach (XmlNode userNode in userNodes)
        //        {
        //            XmlNode node = userNode.ChildNodes.Item(1);
        //            if (node != null)
        //            {
        //                node.InnerText = Convert.ToString(ReadingBy);
        //            }
        //        }

        //        XmlNode specimenNode = xdoc.SelectSingleNode("//ns:CODISImportFile/ns:SPECIMEN", nameSpaceManager);
        //        if (specimenNode != null && specimenNode.Attributes.Count > 0)
        //        {
        //            XmlNode specimenIDNode = xdoc.SelectSingleNode("//ns:CODISImportFile/ns:SPECIMEN/ns:SPECIMENID", nameSpaceManager);

        //            XmlAttribute caseIdAttribute = specimenNode.Attributes["CASEID"];
        //            if (caseIdAttribute != null && !string.IsNullOrEmpty(caseIdAttribute.Value))
        //            {
        //                specimenIDNode.InnerText = caseIdAttribute.Value;
        //                caseIdAttribute.Value = string.Empty;
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        isSucceed = false;
        //    }

        //}

        private string NodeResults(string ChildNodePath)
        {
            string nodeValue = string.Empty;
            return nodeValue = xdoc.SelectSingleNode(ChildNodePath, nameSpaceManager).InnerText;
        }

        //private void NodeWrite(string ChildNodePath, string value)
        //{

        //    try
        //    {
        //        if (!value.Equals("Select"))
        //        {
        //            XmlNode xmlnd = xdoc.SelectSingleNode(ChildNodePath, nameSpaceManager);
        //            xmlnd.InnerText = value;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        //private void createNameSpaceManager(XmlDocument xmlDoc)
        //{
        //    if (xmlDoc.ChildNodes[1].Attributes != null)
        //    {
        //        var xmlns = xmlDoc.ChildNodes[1].Attributes["xmlns"];

        //        if (xmlns != null)
        //        {
        //            nameSpaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
        //            nameSpaceManager.AddNamespace(namespaceName, xmlns.Value);
        //            nameSpaceManager.AddNamespace(namespaceName, "urn:CODISImportFile-schema");
        //        }
        //    }
        //}
    }
}
