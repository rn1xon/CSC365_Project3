using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC365_Project3.Models;
using CsvHelper.Configuration;
using CsvHelper;
using System.Xml.XPath;
using System.Reflection;
using System.Transactions;

namespace CSC365_Project3
{
    internal class CollectionsOperations
    {
        /// <summary>
        /// Combines the data from the 3 Lists by VAERS_ID and returns an IEnumerable of the Anonymous type created from the 3 lists
        /// </summary>
        /// <param name="vaxInfos"></param>
        /// <param name="patientRecs"></param>
        /// <param name="symptoms"></param>
        /// <returns></returns>
        public IEnumerable<VaersReport> GetCombinedDataSet(List<VaxInfo> vaxInfos, List<PatientRec> patientRecs, List<Symptoms> symptoms)
        {
            var joinedList = from vax in vaxInfos
                             join pat in patientRecs on vax.VAERS_ID equals pat.VAERS_ID into vaxPat
                             from pat in vaxPat.DefaultIfEmpty()
                             join sym in symptoms on vax.VAERS_ID equals sym.VAERS_ID into vaxPatSym
                             from sym in vaxPatSym.DefaultIfEmpty()
                             select new VaersReport
                             {
                                 VAERS_ID = vax.VAERS_ID,
                                 RECVDATE = pat?.RECVDATE ?? string.Empty,
                                 STATE = pat?.STATE ?? string.Empty,
                                 AGE_YRS = pat?.AGE_YRS,
                                 CAGE_YR = pat?.CAGE_YR ?? string.Empty,
                                 CAGE_MO = pat?.CAGE_MO ?? string.Empty,
                                 SEX = pat?.SEX ?? string.Empty,
                                 RPT_DATE = pat?.RPT_DATE ?? string.Empty,
                                 SYMPTOM_TEXT = pat?.SYMPTOM_TEXT ?? string.Empty,
                                 DIED = pat?.DIED ?? string.Empty,
                                 DATEDIED = pat?.DATEDIED,
                                 L_THREAT = pat?.L_THREAT ?? string.Empty,
                                 ER_VISIT = pat?.ER_VISIT ?? string.Empty,
                                 HOSPITAL = pat?.HOSPITAL ?? string.Empty,
                                 HOSPDAYS = pat?.HOSPDAYS ?? string.Empty,
                                 X_STAY = pat?.X_STAY ?? string.Empty,
                                 DISABLE = pat?.DISABLE ?? string.Empty,
                                 RECOVD = pat?.RECOVD ?? string.Empty,
                                 VAX_DATE = pat?.VAX_DATE,
                                 ONSET_DATE = pat?.ONSET_DATE ?? string.Empty,
                                 NUMDAYS = pat?.NUMDAYS ?? string.Empty,
                                 LAB_DATA = pat?.LAB_DATA ?? string.Empty,
                                 V_ADMINBY = pat?.V_ADMINBY ?? string.Empty,
                                 V_FUNDBY = pat?.V_FUNDBY ?? string.Empty,
                                 OTHER_MEDS = pat?.OTHER_MEDS ?? string.Empty,
                                 CUR_ILL = pat?.CUR_ILL ?? string.Empty,
                                 HISTORY = pat?.HISTORY ?? string.Empty,
                                 PRIOR_VAX = pat?.PRIOR_VAX ?? string.Empty,
                                 SPLTTYPE = pat?.SPLTTYPE ?? string.Empty,
                                 FORM_VERS = pat?.FORM_VERS ?? string.Empty,
                                 TODAYS_DATE = pat?.TODAYS_DATE ?? string.Empty,
                                 BIRTH_DEFECT = pat?.BIRTH_DEFECT ?? string.Empty,
                                 OFC_VISIT = pat?.OFC_VISIT ?? string.Empty,
                                 ER_ED_VISIT = pat?.ER_ED_VISIT ?? string.Empty,
                                 ALLERGIES = pat?.ALLERGIES ?? string.Empty,
                                 VAX_TYPE = vax.VAX_TYPE,
                                 VAX_MANU = vax.VAX_MANU,
                                 VAX_LOT = vax.VAX_LOT,
                                 VAX_DOSE_SERIES = vax.VAX_DOSE_SERIES,
                                 VAX_ROUTE = vax.VAX_ROUTE,
                                 VAX_SITE = vax.VAX_SITE,
                                 VAX_NAME = vax.VAX_NAME,
                                 SYMPTOM1 = sym?.SYMPTOM1 ?? string.Empty,
                                 SYMPTOMVERSION1 = sym?.SYMPTOMVERSION1 ?? string.Empty,
                                 SYMPTOM2 = sym?.SYMPTOM2 ?? string.Empty,
                                 SYMPTOMVERSION2 = sym?.SYMPTOMVERSION2 ?? string.Empty,
                                 SYMPTOM3 = sym?.SYMPTOM3 ?? string.Empty,
                                 SYMPTOMVERSION3 = sym?.SYMPTOMVERSION3 ?? string.Empty,
                                 SYMPTOM4 = sym?.SYMPTOM4 ?? string.Empty,
                                 SYMPTOMVERSION4 = sym?.SYMPTOMVERSION4 ?? string.Empty,
                                 SYMPTOM5 = sym?.SYMPTOM5 ?? string.Empty,
                                 SYMPTOMVERSION5 = sym?.SYMPTOMVERSION5 ?? string.Empty
                             };

            return joinedList;
        }

        public static List<AgeBucket> ageBuckets = new List<AgeBucket>
            {
                new() { Label = "< 1", MinAge = 0, MaxAge = 1 },
                new() { Label = "1-3", MinAge = 1, MaxAge = 3 },
                new() { Label = "4-11", MinAge = 4, MaxAge = 11 },
                new() { Label = "12-18", MinAge = 12, MaxAge = 18 },
                new() { Label = "19-30", MinAge = 19, MaxAge = 30 },
                new() { Label = "31-40", MinAge = 31, MaxAge = 40 },
                new() { Label = "41-50", MinAge = 41, MaxAge = 50 },
                new() { Label = "51-60", MinAge = 51, MaxAge = 60 },
                new() { Label = "61-70", MinAge = 61, MaxAge = 70 },
                new() { Label = "71-80", MinAge = 71, MaxAge = 80 },
                new() { Label = "> 80", MinAge = 80, MaxAge = 130 }
            };

        internal List<VaersGrouped> CreateMLDataSet(IEnumerable<VaersReport> joinedList, Dictionary<string, string> cats)
        {
            List<VaersGrouped> rtn = [];
            var groupedData = joinedList.GroupBy(x => x.VAERS_ID);

            foreach (var group in groupedData)
            {
                VaersReport f = group.First();
                VaersGrouped item = new();
                item.VAERS_ID = group.Key;
                item.VAX_MANU = f.VAX_MANU;
                item.RECVDATE = f.RECVDATE;
                item.AGE_YRS = f.AGE_YRS;
                item.SEX = f.SEX;
                item.DIED = f.DIED;
                item.DATEDIED = f.DATEDIED;
                item.VAX_DATE = f.VAX_DATE;
                item.Symptoms = group.SelectMany(x => new List<string> { x.SYMPTOM1, x.SYMPTOM2, x.SYMPTOM3, x.SYMPTOM4, x.SYMPTOM5 })
                                     .ToList()
                                     .Where(x => !string.IsNullOrEmpty(x))
                                     .ToList();
                item.SymptomCats = item.Symptoms
                                    .Select(x =>
                                    {
                                        if (cats.TryGetValue(x, out var value))
                                        {
                                            return value; // Return the value if the key exists
                                        }
                                        return null; // Return null (or a default value) if the key doesn't exist
                                    })
                                    .Where(cat => cat != null).Distinct() // Filter out null values, remove duplicate categories
                                    .ToList();
                rtn.Add(item);

            }
            return rtn;
        }

        internal List<VaersTransaction> CreateTransactionData(List<VaersGrouped> mLData)
        {
            List<VaersTransaction> rtn = [];

            foreach (var v in mLData)
            {
                VaersTransaction item = new();
                item.VAERS_ID = v.VAERS_ID;

                item.AllergicandImmuneReactions = v.SymptomCats.Any(x => x.Replace(" ", "") == "AllergicandImmuneReactions");
                item.CardiovascularFindings = v.SymptomCats.Any(x => x.Replace(" ", "") == "CardiovascularFindings");
                item.DermatologicSymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "DermatologicSymptoms");
                item.ENTSymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "ENTSymptoms");
                item.GastrointestinalSymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "GastrointestinalSymptoms");
                item.GeneralSymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "GeneralSymptoms");
                item.GenitourinarySymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "GenitourinarySymptoms");
                item.InfectiousSymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "InfectiousSymptoms");
                item.LaboratoryFindings = v.SymptomCats.Any(x => x.Replace(" ", "") == "LaboratoryFindings");
                item.MusculoskeletalSymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "MusculoskeletalSymptoms");
                item.NeurologicalSymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "NeurologicalSymptoms");
                item.OphthalmologicSymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "OphthalmologicSymptoms");
                item.ProceduralandDeviceRelatedIssues = v.SymptomCats.Any(x => x.Replace(" ", "") == "ProceduralandDeviceRelatedIssues");
                item.PsychiatricSymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "PsychiatricSymptoms");
                item.RespiratorySymptoms = v.SymptomCats.Any(x => x.Replace(" ", "") == "RespiratorySymptoms");
                item.Uncategorized = v.SymptomCats.Any(x => x.Replace(" ", "") == "Uncategorized");

                item.Male = v.SEX.Equals("M", StringComparison.OrdinalIgnoreCase);
                item.Female = v.SEX.Equals("F", StringComparison.OrdinalIgnoreCase);

                item.LessThan1 = v.AGE_YRS >= 0 && Math.Floor(v.AGE_YRS.Value) < 1;
                item.Oneto3 = v.AGE_YRS >= 1 && Math.Floor(v.AGE_YRS.Value) <= 3;
                item.Fourto11 = v.AGE_YRS >= 4 && Math.Floor(v.AGE_YRS.Value) <= 11;
                item.Twelveto18 = v.AGE_YRS >= 12 && Math.Floor(v.AGE_YRS.Value) <= 18;
                item.Nineteento30 = v.AGE_YRS >= 19 && Math.Floor(v.AGE_YRS.Value) <= 30;
                item.ThirtyOneto40 = v.AGE_YRS >= 31 && Math.Floor(v.AGE_YRS.Value) <= 40;
                item.FourtyOneto50 = v.AGE_YRS >= 41 && Math.Floor(v.AGE_YRS.Value) <= 50;
                item.FiftyOneto60 = v.AGE_YRS >= 51 && Math.Floor(v.AGE_YRS.Value) <= 60;
                item.SixtyOneto70 = v.AGE_YRS >= 61 && Math.Floor(v.AGE_YRS.Value) <= 70;
                item.SeventyOneto80 = v.AGE_YRS >= 71 && Math.Floor(v.AGE_YRS.Value) <= 80;
                item.GreaterThan80 = v.AGE_YRS > 80;

                item.PFIZER_BIONTECH = v.VAX_MANU == "PFIZER/BIONTECH";
                item.UNKNOWN_MANUFACTURER = v.VAX_MANU == "UNKNOWN MANUFACTURER";
                item.MODERNA = v.VAX_MANU == "MODERNA";
                item.JANSSEN = v.VAX_MANU == "JANSSEN";
                item.NOVAVAX = v.VAX_MANU == "NOVAVAX";

                item.DiedLessThanWeek = v.DaysDiedAfterVax.HasValue && v.DaysDiedAfterVax < 7;
                item.DiedWeektoMonth = v.DaysDiedAfterVax.HasValue && v.DaysDiedAfterVax >= 7 && v.DaysDiedAfterVax < 30;
                item.DiedMonthto6Months = v.DaysDiedAfterVax.HasValue && v.DaysDiedAfterVax >= 30 && v.DaysDiedAfterVax < 180;
                item.DiedSixMonthstoYear = v.DaysDiedAfterVax.HasValue && v.DaysDiedAfterVax >= 180 && v.DaysDiedAfterVax <= 365;
                item.DiedGreaterThanYear = v.DaysDiedAfterVax.HasValue && v.DaysDiedAfterVax > 365;

                rtn.Add(item);
            }
            return rtn;
        }



        public Dictionary<string, (double Support, double Confidence)> AnalyzeTransactionDataSet(
            List<VaersTransaction> alltransactions, double minSupport, double minConfidence, bool femaleOnly, double initialMinSupport)
        {
            List<VaersTransaction> transactions = alltransactions.Where(x => x.Female == femaleOnly && x.Male == !femaleOnly).ToList();
            int totalRows = transactions.Count;

            // Step 1: Identify frequent single-item properties
            var frequentItemsets = typeof(VaersTransaction).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(bool) && p.Name != "Male" && p.Name != "Female")
                .Where(p =>
                {
                    int trueCount = transactions.Count(t => (bool)p.GetValue(t));
                    return (double)trueCount / totalRows > initialMinSupport;
                })
                .Select(p => new List<PropertyInfo> { p }) // Wrap each property in a list (itemset)
                .ToList();

            var results = new Dictionary<string, (double Support, double Confidence)>(); // For rules

            // Step 2: Iteratively generate larger itemsets
            for (int k = 2; frequentItemsets.Any(); k++)
            {
                var candidateItemsets = GenerateCandidateItemsets(frequentItemsets);

                frequentItemsets = candidateItemsets
                    .Where(itemset =>
                    {
                        int trueCount = transactions.Count(t => itemset.All(p => (bool)p.GetValue(t)));
                        double support = (double)trueCount / totalRows;
                        if (support > minSupport)
                        {
                            // Generate rules from this itemset
                            foreach (var consequent in itemset)
                            {
                                var antecedent = itemset.Where(p => p != consequent).ToList();
                                int antecedentCount = transactions.Count(t => antecedent.All(p => (bool)p.GetValue(t)));
                                double confidence = (double)trueCount / antecedentCount; // Calculate confidence

                                if (confidence >= minConfidence)
                                {
                                    string rule = $"{{{string.Join(" AND ", antecedent.Select(p => p.Name))}}} --> {{{consequent.Name}}}";
                                    results[rule] = (support, confidence);
                                }
                            }

                            return true; // Keep the itemset as frequent
                        }
                        return false; // Prune itemset if support is below threshold
                    })
                    .ToList();
            }

            return results;
        }


        public static List<List<PropertyInfo>> GenerateCandidateItemsets(List<List<PropertyInfo>> frequentItemsets)
        {
            var candidates = new List<List<PropertyInfo>>();
            for (int i = 0; i < frequentItemsets.Count; i++)
            {
                for (int j = i + 1; j < frequentItemsets.Count; j++)
                {
                    var combined = frequentItemsets[i].Union(frequentItemsets[j]).Distinct().ToList();
                    if (combined.Count == frequentItemsets[i].Count + 1) // Ensure valid combination
                    {
                        // Prune if any subset is not frequent
                        if (AllSubsetsAreFrequent(combined, frequentItemsets))
                        {
                            candidates.Add(combined);
                        }
                    }
                }
            }
            return candidates;
        }

        private static bool AllSubsetsAreFrequent(List<PropertyInfo> candidate, List<List<PropertyInfo>> frequentItemsets)
        {
            return candidate
                .Select((_, index) => candidate.Where((_, idx) => idx != index).ToList())
                .All(subset => frequentItemsets.Any(frequent => frequent.SequenceEqual(subset)));
        }


    }
}
