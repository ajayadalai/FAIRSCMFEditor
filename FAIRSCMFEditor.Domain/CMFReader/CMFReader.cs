
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FAIRSCMFEditor.Domain
{
    public class CMFReader : ProfileReader
    {
        #region Constants and Member Variables
        const int RECOGNIZE_CHAR_LIMIT = 100;
        static Regex recognizeRE_ = new Regex(@"<[\s]*CODISImportFile");

        // What the source lab should be set to if the below rules are to be applied
        static Regex expectedSourceLabRE_ = new Regex(@"i[0-9]{4}", RegexOptions.IgnoreCase);

        // What BatchANDERun/COGS/GF generate independently
        static Regex expectedCmfInternalNameRE_ = new Regex(@"R([0-9]+)_(P[0-9]+)");
        static Regex expectedCmfLaneRE_ = new Regex(@"_L([0-9]+[0-9]*)(.xml|_)");
        #endregion

        #region Methods
        /// <summary>
        /// ReadProfiles
        /// 
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>ProfileReader.Result</returns>
        public override ProfileReader.Result ReadProfiles(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException();
            }

            var res = new ProfileReader.Result();

            if (!stream.CanSeek)
            {
                throw new ArgumentException("Need seekable stream (CanSeek returns true)");
            }

            try
            {
                stream.Seek(0, SeekOrigin.Begin);

                // If not an XML file, don't bother
                var filepath = string.Empty;
                var fs = stream as FileStream;
                if (fs != null)
                {
                    filepath = fs.Name;
                    if (".xml" != Path.GetExtension(filepath).ToLower())
                    {
                        // Not a CMF
                        res.Recognized = false;
                        return res;
                    }
                }

                // Do not put reader1/2 in using() block - don't want underlying
                // stream to be closed.
                //
                // See:
                // http://stackoverflow.com/questions/1862261/can-you-keep-a-streamreader-from-disposing-the-underlying-stream
                //
                var reader1 = new StreamReader(stream);

                // Read in a portion of the file to see if we recognize it
                var blockbytes = new char[RECOGNIZE_CHAR_LIMIT];
                reader1.Read(blockbytes, 0, blockbytes.Length);
                var blockstr = new string(blockbytes);

                // Rewind to beginning no matter the outcome
                stream.Seek(0, SeekOrigin.Begin);

                // Check to see if it matches beginning of CMF
                if (!recognizeRE_.IsMatch(blockstr))
                {
                    // Not a CMF
                    res.Recognized = false;
                    return res;
                }
                // We think it's a CMF
                res.Recognized = true;

                // Parse XML
                var reader2 = new StreamReader(stream);
                var cmf = XElement.Load(reader2);
                XNamespace ns = cmf.Attribute("xmlns").Value;

                // Properties common to all specimens of a CMF batch
                var srcLab = string.Empty;
                var submitByUserID = string.Empty;
                var submitDatetime = DateTime.MinValue;
                var batchID = string.Empty;
                var kit = string.Empty;

                foreach (var inst in cmf.Descendants(ns + "SOURCELAB"))
                {
                    srcLab = inst.Value;
                    break;
                }

                foreach (var op in cmf.Descendants(ns + "SUBMITBYUSERID"))
                {
                    submitByUserID = op.Value;
                    break;
                }

                foreach (var dt in cmf.Descendants(ns + "SUBMITDATETIME"))
                {
                    if (DateTime.TryParse(dt.Value, out submitDatetime))
                    {
                        break;
                    }
                }

                foreach (var bat in cmf.Descendants(ns + "BATCHID"))
                {
                    batchID = bat.Value;
                    break;
                }

                foreach (var k in cmf.Descendants(ns + "KIT"))
                {
                    kit = k.Value;
                    break;
                }

                // Iterator through each specimen
                foreach (var sp in cmf.Descendants(ns + "SPECIMEN"))
                {
                    var profile = new Profile();

                    // Assign common properties to this specimen's profile
                    profile.Instrument = srcLab;
                    profile.Operator = submitByUserID;
                    profile.Timestamp = submitDatetime;
                    profile.ChipId = batchID;
                    profile.Kit = kit;
                    var regexInstr = new Regex(@"[Ii][0-9][0-9][0-9][0-9]");
                    var regexANDE = new Regex(@"AND");
                    profile.RunId = (regexInstr.Match(srcLab).Success || regexANDE.Match(srcLab).Success ? submitDatetime + srcLab : "None");

                    // Assign specimen specific values
                    foreach (var att in sp.Attributes("CASEID"))
                    {
                        profile.SampleId = att.Value;
                        break;
                    }

                    foreach (var spid in sp.Descendants(ns + "SPECIMENID"))
                    {
                        profile.RFID = spid.Value;
                        if (string.IsNullOrEmpty(profile.SampleId))
                        {
                            profile.SampleId = spid.Value;
                        }
                        break;
                    }

                    var calls = new Dictionary<string, HashSet<string>>();
                    foreach (var desc in sp.Descendants(ns + "LOCUS"))
                    {
                        var locus = string.Empty;
                        foreach (var locusName in desc.Descendants(ns + "LOCUSNAME"))
                        {
                            locus = locusName.Value;
                            break;
                        }
                        foreach (var allele in desc.Descendants(ns + "ALLELEVALUE"))
                        {
                            profile.AddAllele(locus, allele.Value);
                        }
                    }

                    res.Profiles.Add(profile);
                }

            }
            catch (Exception ex)
            {
                res.Error = true;
                res.Message = ex.Message;
            }

            return res;
        }
        #endregion
    }
}
