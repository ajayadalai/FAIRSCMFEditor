using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
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
                            _sampleInfoFileName = fn;
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

            if (locus.Key.ToLower() == "pentae")
                sbContent.AppendLine("Penta E");
            else if (locus.Key.ToLower() == "pentad")
                sbContent.AppendLine("Penta D");
            else
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
}
