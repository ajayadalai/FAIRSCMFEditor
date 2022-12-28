using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public class Profile
    {
        #region Member Variables
        string runId_ = string.Empty;
        string lane_ = string.Empty;

        string instrument_ = string.Empty;
        string operator_ = string.Empty;
        string sampleId_ = string.Empty;
        string kit_ = string.Empty;
        string rfId_ = string.Empty;
        string chipId_ = string.Empty;
        DateTime timestamp_ = DateTime.MinValue;
        string notes_ = string.Empty;
        string assayType_ = "Other"; // assay type (determiner 4C or 6C profile)

        // Locus -> list of Alleles
        Dictionary<string, HashSet<string>> lociCalls_ = new Dictionary<string, HashSet<string>>();
        #endregion

        #region Static Member Variables
        static List<ProfileReader> readers_ = new List<ProfileReader>
        {
            new CMFReader(),
          //  new GeneMarkerAlleleReportReader()
        };

        public static Profile Unknown = new Profile("???");

        static Regex HDR_ID = new Regex("(Sample ID)|(SampleID)|(Sample Name)|(Truth ID)", RegexOptions.IgnoreCase);
        static Regex HDR_NOTES = new Regex("(Notes)|(Comments)", RegexOptions.IgnoreCase);
        static Regex SOURCELAB = new Regex("SOURCELAB", RegexOptions.IgnoreCase);
        static Regex SUBMITBYUSERID = new Regex("SUBMITBYUSERID", RegexOptions.IgnoreCase);
        static Regex SUBMITDATETIME = new Regex("SUBMITDATETIME", RegexOptions.IgnoreCase);
        #endregion

        #region Properties
        public IDictionary<string, HashSet<string>> LociCalls { get { return lociCalls_; } }

        #region CODIS Properties
        public string Kit { get { return kit_; } set { kit_ = value; } }
        public string CaseId { get { return sampleId_; } } // PHYX Should this be Source ID on non-NetBio CMF's??
        public string SpecimenId { get { return rfId_; } }
        public DateTime SubmitDateTime { get { return timestamp_; } }
        public string SourceLab { get { return instrument_; } }
        public string SubmitByUserId { get { return operator_; } }
        public string BatchId { get { return chipId_; } }
        #endregion

        #region NetBio specific Properties
        public string RunId { get { return runId_; } set { runId_ = value; } }
        public string Lane { get { return lane_; } set { lane_ = value; } }
        public string Instrument { get { return instrument_; } set { instrument_ = value; } }
        public string Operator { get { return operator_; } set { operator_ = value; } }
        public string SampleId { get { return sampleId_; } internal set { sampleId_ = value; } }
        public string RFID { get { return rfId_; } set { rfId_ = value; } }
        public string ChipId { get { return chipId_; } set { chipId_ = value; } }
        public DateTime Timestamp { get { return timestamp_; } set { timestamp_ = value; } }
        public bool IsTimestampValid { get { return IsValidTimestamp(timestamp_); } }
        public string Notes { get { return notes_; } set { notes_ = value; } }
        public string AssayType { get { return assayType_; } set { assayType_ = value; } }
        #endregion
        #endregion

        #region Constructors
        internal Profile() { }

        /// <summary>
        /// Profile
        /// 
        /// </summary>
        /// <param name="sampleId">string</param>
        public Profile(string sampleId)
        {
            sampleId_ = sampleId;
        }

        /// <summary>
        /// Profile
        /// 
        /// </summary>
        /// <param name="sampleId">string</param>
        /// <param name="notes">string</param>
        public Profile(string sampleId, string notes)
        {
            sampleId_ = sampleId;
            notes_ = notes;
        }

        /// <summary>
        /// Profile
        /// 
        /// </summary>
        /// <param name="sampleId"></param>
        /// <param name="instrument"></param>
        /// <param name="op"></param>
        /// <param name="kit"></param>
        /// <param name="rfId"></param>
        /// <param name="chipId"></param>
        /// <param name="timestamp"></param>
        /// <param name="notes"></param>
        /// <param name="assayType"></param>
        public Profile(string sampleId, string instrument, string op, string kit, string rfId, string chipId, DateTime timestamp, string notes, string assayType = "PP16")
        {
            instrument_ = instrument;
            operator_ = op;
            sampleId_ = sampleId;
            kit_ = kit;
            rfId_ = rfId;
            chipId_ = chipId;
            timestamp_ = timestamp;
            notes_ = notes;
            assayType_ = assayType;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Equals
        /// 
        /// </summary>
        /// <param name="other">Profile</param>
        /// <returns>bool</returns>
        public bool Equals(Profile other)
        {
            ////&& operator_ == other.operator_
            ////&& timestamp_ == other.timestamp_

            bool returnVal = (sampleId_ == other.sampleId_
                && rfId_ == other.rfId_
                && instrument_ == other.instrument_
                && chipId_ == other.chipId_
                && kit_ == other.kit_
                && IsSameIdentity(other)
                && assayType_ == other.assayType_);

            return returnVal;
        }

        /// <summary>
        /// IsSameRun
        /// 
        /// </summary>
        /// <param name="other">Profile</param>
        /// <returns>bool</returns>
        public bool IsSameRun(Profile other)
        {
            return Equals(other);
        }

        /// <summary>
        /// IsSameIdentity
        /// 
        /// </summary>
        /// <param name="other">Profile</param>
        /// <returns>bool</returns>
        public bool IsSameIdentity(Profile other)
        {
            foreach (var kvp in lociCalls_)
            {
                HashSet<string> alleles = kvp.Value;
                HashSet<string> other_alleles;
                if (!other.lociCalls_.TryGetValue(kvp.Key, out other_alleles))
                {
                    return false;
                }

                if (!alleles.SetEquals(other_alleles))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// IsValidTimestamp
        /// 
        /// </summary>
        /// <param name="ts">DateTime</param>
        /// <returns>bool</returns>
        public static bool IsValidTimestamp(DateTime ts)
        {
            return ts.CompareTo(DateTime.MinValue) != 0 && ts.CompareTo(DateTime.MaxValue) != 0;
        }

        /// <summary>
        /// GetNumLociCalled
        /// 
        /// </summary>
        /// <param name="loci">Loci</param>
        /// <returns>int</returns>
        public int GetNumLociCalled(Loci loci)
        {
            var tally = 0;

            foreach (var kvp in lociCalls_)
            {
                var locus = kvp.Key;
                if (loci.Contains(locus))
                {
                    ++tally;
                }
            }

            return tally;
        }

        /// <summary>
        /// SortedAlleles
        /// 
        /// </summary>
        /// <param name="locus">string</param>
        /// <returns>List of string</returns>
        public List<string> SortedAlleles(string locus)
        {
            var alleles = new List<string>();

            HashSet<string> set;
            if (!lociCalls_.TryGetValue(locus, out set))
            {
                return alleles;
            }

            alleles.AddRange(set.ToList());

            // Sort alleles in ascending order for sanity
            alleles.Sort((a, b) =>
            {
                float fa;
                float fb;

                var aValue = a.StartsWith("<") || a.StartsWith(">") ? a.Substring(1).Trim() : a;
                var bValue = b.StartsWith("<") || b.StartsWith(">") ? b.Substring(1).Trim() : b;
                if (float.TryParse(aValue, out fa) && float.TryParse(bValue, out fb))
                {
                    // x10 so we don't worry about floating point comparison
                    int ia = (int)(fa * 10.0 + .5);
                    int ib = (int)(fb * 10.0 + .5);
                    return Comparer<int>.Default.Compare(ia, ib);
                }
                else
                {
                    return Comparer<string>.Default.Compare(a, b);
                }
            });

            return alleles;
        }

        /// <summary>
        /// MatchScore
        /// 
        /// Ex. 1
        /// Truth    12  13           Score  
        /// Donor1   12  13    1  1  =    2
        ///      2   12  12    1  0  =    1  << only count once for partial match
        ///      3   12  14    1 -2  =   -1
        ///      4   14  16   -2 -2  =   -4
        ///      5    x   x    0  0  =    0
        ///      6   17  17   -2 -2  =   -4
        ///
        /// Ex. 2
        /// Truth    12  12           Score
        /// Donor1   12  13    1 -2  =   -1
        ///      2   12  12    1  1  =    2
        ///      3   12  14    1 -2  =   -1
        ///      4   14  16   -2 -2  =   -4
        ///      5    x   x    0  0  =    0
        ///      6   17  17   -2 -2  =   -4
        ///
        /// </summary>
        /// <param name="expectedLoci">Loci</param>
        /// <param name="truth">Profile</param>
        /// <returns>int</returns>
        public int MatchScore(Loci expectedLoci, Profile truth)
        {
            var score = 0;
            foreach (var locus in expectedLoci)
            {
                HashSet<string> donor_alleles;
                if (!lociCalls_.TryGetValue(locus, out donor_alleles))
                {
                    // No donors - score is 0
                    continue;
                }

                HashSet<string> truth_alleles;
                if (!truth.lociCalls_.TryGetValue(locus, out truth_alleles))
                {
                    // No likely to get here - score is 0
                    continue;
                }

                // Duplicate homozygote donor for scoring purposes if
                // not a partial match (see comment above)
                var donor_alleles_explicit = donor_alleles.ToList();
                if (donor_alleles.Count == 1 && (truth_alleles.Count == 1 || !truth_alleles.Contains(donor_alleles.First())))
                {
                    donor_alleles_explicit.Add(donor_alleles_explicit.First());
                }

                var score_for_locus = 0;
                foreach (var allele in donor_alleles_explicit)
                {
                    score_for_locus += ((truth_alleles.Contains(allele)) ? 1 : 2);
                }
                score += score_for_locus;
            }

            return score;
        }

        /// <summary>
        /// AddAlleleNoCheck
        /// 
        /// </summary>
        /// <param name="locus"></param>
        /// <param name="allele"></param>
        internal void AddAlleleNoCheck(string locus, string allele)
        {
            HashSet<string> alleles;
            if (!lociCalls_.TryGetValue(locus, out alleles))
            {
                alleles = new HashSet<string>();
                lociCalls_.Add(locus, alleles);
            }
            alleles.Add(allele);
        }

        /// <summary>
        /// AddAllele
        /// 
        /// </summary>
        /// <param name="specifiedLocus"></param>
        /// <param name="specifiedAllele"></param>
        public void AddAllele(string specifiedLocus, string specifiedAllele)
        {
            var locus = Loci.NormalizeLocusName(specifiedLocus);

            //if (!Loci.Known.Contains(locus))
            //{
            //    throw new ApplicationException(string.Format("Unknown locus {0}.", specifiedLocus));
            //}
            if (Loci.Known.Contains(locus))
            {
                var allele = specifiedAllele.ToUpper().Trim().Replace(" ", "");
                var dummy = 0f;
                // if (string.IsNullOrEmpty(allele) || (!float.TryParse(allele, out dummy) && allele != "X" && allele != "Y"))
                if (string.IsNullOrEmpty(allele) || ((allele != "X" && allele != "Y") && ((allele.StartsWith(">") || allele.StartsWith("<")) ? (!float.TryParse(allele.Substring(1), out dummy)) :
                    (!float.TryParse(allele, out dummy)))))
                {
                    throw new ApplicationException(string.Format("Invalid allele {0}.", specifiedAllele.Trim()));
                }

                AddAlleleNoCheck(locus, allele);
            }
        }

        /// <summary>
        /// ReadFromFile
        /// 
        /// </summary>
        /// <param name="path">string</param>
        /// <returns>ProfileReader.Result</returns>
        public static ProfileReader.Result ReadFromFile(string path)
        {
            ProfileReader.Result result = new ProfileReader.Result();
            try
            {
                using (var fs = File.Open(path, FileMode.Open))
                {
                    result = ReadFromStream(fs);
                }
            }
            catch (System.IO.IOException)
            {
                result.Message = "File you are trying to import is opened. Please close the file and try again.";
                result.Error = true;
            }
            catch (Exception ex)
            {
                result.Error = true;
            }
            return result;
        }

        /// <summary>
        /// ReadFromStream
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>ProfileReader.Result</returns>
        public static ProfileReader.Result ReadFromStream(Stream stream)
        {
            foreach (var reader in readers_)
            {
                var res = reader.ReadProfiles(stream);
                if (!res.Recognized)
                {
                    continue;
                }
                return res;
            }

            return ProfileReader.Result.None;
        }

       
        #endregion
    }
}
