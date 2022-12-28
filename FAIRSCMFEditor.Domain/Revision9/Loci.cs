using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FAIRSCMFEditor.Domain
{
    public class Loci : IEnumerable<string>
    {
        public static readonly Loci CODIS13 = new Loci("CODIS13",
            new List<string> {
                "D3S1358"
            ,   "TH01"
            ,   "D21S11"
            ,   "D18S51"
            ,   "D5S818"
            ,   "D13S317"
            ,   "D7S820"
            ,   "D16S539"
            ,   "CSF1PO"
            ,   "vWA"
            ,   "D8S1179"
            ,   "TPOX"
            ,   "FGA"
            }
            );

        public static readonly Loci PowerPlex16 = new Loci("PowerPlex16",
            new List<string> {
                "D3S1358"
            ,   "TH01"
            ,   "D21S11"
            ,   "D18S51"
            ,   "PentaE"
            ,   "D5S818"
            ,   "D13S317"
            ,   "D7S820"
            ,   "D16S539"
            ,   "CSF1PO"
            ,   "PentaD"
            ,   "AMEL"
            ,   "vWA"
            ,   "D8S1179"
            ,   "TPOX"
            ,   "FGA"
            }
            );

        public static readonly Loci PowerPlexFusion = new Loci("PowerPlexFusion",
            new List<string> {
                "D3S1358"
              , "D1S1656"
              , "D2S441"
              , "D10S1248"
              , "D13S317"
              , "PentaE"
              , "D16S539"
              , "D18S51"
              , "D2S1338"
              , "CSF1PO"
              , "PentaD"
              , "TH01"
              , "vWA"
              , "D21S11"
              , "D7S820"
              , "D5S818"
              , "TPOX"
              , "DYS391"
              , "AMEL"
              , "D8S1179"
              , "D12S391"
              , "D19S433"
              , "FGA"
              , "D22S1045"
            }
            );

        public static readonly Loci FlexPlex = new Loci("FlexPlex",
            new List<string> { //"Am"
                "AMEL"
              , "D3S1358"
              , "D1S1656"
              , "D2S441"
              , "D10S1248"
              , "D13S317"
              , "PentaE"
              , "D16S539"
              , "D18S51"
              , "D2S1338"
              , "CSF1PO"
              , "D6S1043"
              , "TH01"
              , "vWA"
              , "D21S11"
              , "D7S820"
              , "D5S818"
              , "TPOX"
              , "DYS391"
              , "D8S1179"
              , "D12S391"
              , "D19S433"
              , "SE33"
              , "D22S1045"
              , "FGA"
              , "DYS576"
              , "DYS570"
            }
            );

        public static readonly Loci Other = new Loci("Other",
            new List<string> { //"Am"
                "AMEL"
              , "D3S1358"
              , "D1S1656"
              , "D2S441"
              , "D10S1248"
              , "D13S317"
              , "PentaE"
              , "D16S539"
              , "D18S51"
              , "D2S1338"
              , "CSF1PO"
              , "D6S1043"
              , "TH01"
              , "vWA"
              , "D21S11"
              , "D7S820"
              , "D5S818"
              , "TPOX"
              , "DYS391"
              , "D8S1179"
              , "D12S391"
              , "D19S433"
              , "SE33"
              , "D22S1045"
              , "FGA"
              , "DYS576"
              , "DYS570"
              , "PentaD"
            }
    );

        public static readonly Loci GlobalFiler = new Loci("GlobalFiler",
            new List<string> {
                "D3S1358"
              , "vWA"
              , "D16S539"
              , "CSF1PO"
              , "TPOX"
              , "Yindel"
              , "AMEL"
              , "D8S1179"
              , "D21S11"
              , "D18S51"
              , "DYS391"
              , "D2S441"
              , "D19S433"
              , "TH01"
              , "FGA"
              , "D22S1045"
              , "D5S818"
              , "D13S317"
              , "D7S820"
              , "SE33"
              , "D10S1248"
              , "D1S1656"
              , "D12S391"
              , "D2S1338"
            }
            );

        public static Loci GetLoci(string lociName)
        {
            var type = typeof(Loci);
            Loci ret = null;
            try
            {
                var target = type.GetField(lociName, BindingFlags.Public | BindingFlags.Static);
                ret = target.GetValue(null) as Loci;
            }
            catch (Exception)
            {
            }
            return ret;
        }

        private static Loci known_;
        public static Loci Known
        {
            get
            {
                if (known_ == null)
                {
                    var all = new List<string>();

                    all.AddRange(PowerPlex16);
                    all.AddRange(PowerPlexFusion);
                    all.AddRange(GlobalFiler);
                    all.AddRange(FlexPlex);
                    all.Add("D6S1043");

                    var set = new HashSet<string>(all);

                    known_ = new Loci("Known", set.ToList());
                }

                return known_;
            }
        }

        string name_;
        List<string> inOrder_;
        HashSet<string> searchSet_;

        public Loci(string name, List<string> inOrder)
        {
            name_ = name;
            inOrder_ = inOrder;
            searchSet_ = new HashSet<string>(inOrder_);
        }

        public string Name { get { return name_; } }

        public int Count { get { return inOrder_.Count; } }

        public bool Contains(string locus)
        {
            if (locus == null)
            {
                return false;
            }

            locus = locus.Trim();
            if (string.IsNullOrEmpty(locus))
            {
                return false;
            }
            return searchSet_.Contains(locus);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return inOrder_.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return inOrder_.GetEnumerator();
        }

        public static string NormalizeLocusName(string locusName)
        {
            locusName = locusName.Trim();
            string upper = locusName.ToUpper();
            if (upper.Equals("AM")
             || upper.Equals("AMEL")
             || upper.Equals("AMELOGENIN"))
            {
                return "AMEL";
            }
            else if (upper.Equals("PENTA D")
                  || upper.Equals("PENTAD")
                  || upper.Equals("PENTA_D"))
            {
                return "PentaD";
            }
            else if (upper.Equals("PENTA E")
                  || upper.Equals("PENTAE")
                  || upper.Equals("PENTA_E"))
            {
                return "PentaE";
            }
            else if (upper.Equals("D7S1820"))
            {
                // Handle Analogic typo.  Yay.
                return "D7S820";
            }

            return locusName;
        }
    }
}
