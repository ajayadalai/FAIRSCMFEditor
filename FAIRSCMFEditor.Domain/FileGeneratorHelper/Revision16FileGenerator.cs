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

            if (profiles != null && profiles.Count > 0)
            {
                if (!Directory.Exists(EditedFilePath))
                {
                    Directory.CreateDirectory(EditedFilePath);
                }

                string txtfileName = GetEditedFileName(EditedFilePath) + "_Final_" + timestamp + ".xml";
                string desktoptxtfileName = GetEditedFileName(DesktopEditedPath) + "_Final_" + timestamp + ".xml";
                StringBuilder sb = new StringBuilder();
                StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(sb, Encoding.UTF8);
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.CloseOutput = true;


                using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
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
                string xml = sb.ToString(); //Encoding.UTF8.GetString(memoryStream.ToArray());
                File.WriteAllText(txtfileName, xml, Encoding.ASCII);
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
            writer.WriteElementString("SPECIMENID", profile.SampleId);// NodeResults("//ns:CODISImportFile/ns:SPECIMEN/ns:SPECIMENID"));
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


        private string GetNodeAttributeValue(string nodePath, string attributeName)
        {
            string nodeValue = string.Empty;

            return nodeValue = xdoc.SelectSingleNode(nodePath, nameSpaceManager).Attributes[attributeName] != null ?
                xdoc.SelectSingleNode(nodePath, nameSpaceManager).Attributes[attributeName].Value : "";
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
            string locusKey = locusDetails.Key;
            if (locusKey.ToLower().Equals("amel"))
            {
                locusKey = "Amelogenin";
            }
            else if(locusKey.ToLower().Equals("pentae"))
            {
                locusKey = "Penta E";
            }

            writer.WriteElementString("LOCUSNAME", locusKey);
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

        private void CopyFile(string sourcePath, string targetPath, string DesktopEditedPath)
        {
            if (!Directory.Exists(DesktopEditedPath))
            {
                Directory.CreateDirectory(DesktopEditedPath);
            }

            System.IO.File.Copy(sourcePath, targetPath, true);
        }

    }
}
