using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC365_Project3.Models;

namespace CSC365_Project3.Models
{
    internal class VaersTransaction
    {
        public int VAERS_ID { get; set; }
        public bool NeurologicalSymptoms { get; set; }
        public bool DermatologicSymptoms { get; set; }
        public bool GeneralSymptoms { get; set; }
        public bool Uncategorized { get; set; }
        public bool CardiovascularFindings { get; set; }
        public bool LaboratoryFindings { get; set; }
        public bool InfectiousSymptoms { get; set; }
        public bool GastrointestinalSymptoms { get; set; }
        public bool RespiratorySymptoms { get; set; }
        public bool MusculoskeletalSymptoms { get; set; }
        public bool OphthalmologicSymptoms { get; set; }
        public bool ENTSymptoms { get; set; }
        public bool PsychiatricSymptoms { get; set; }
        public bool AllergicandImmuneReactions { get; set; }
        public bool ProceduralandDeviceRelatedIssues { get; set; }
        public bool GenitourinarySymptoms { get; set; }
        public bool Male {  get; set; }
        public bool Female { get; set; }
        public bool LessThan1 { get; set; }
        public bool Oneto3 { get; set; }
        public bool Fourto11 { get; set; }
        public bool Twelveto18 { get; set; }
        public bool Nineteento30 { get; set; }
        public bool ThirtyOneto40 { get; set; }
        public bool FourtyOneto50 { get; set; }
        public bool FiftyOneto60 { get; set; }
        public bool SixtyOneto70 { get; set; }
        public bool SeventyOneto80 { get; set; }
        public bool GreaterThan80 { get; set; }
        public bool PFIZER_BIONTECH { get; set; }
        public bool UNKNOWN_MANUFACTURER { get; set; }
        public bool MODERNA { get; set; }
        public bool JANSSEN { get; set; }
        public bool NOVAVAX { get; set; }
        public bool DiedLessThanWeek { get; set; }
        public bool DiedWeektoMonth { get; set; }
        public bool DiedMonthto6Months { get; set; }
        public bool DiedSixMonthstoYear { get;set; }
        public bool DiedGreaterThanYear { get; set; }

    }
}
