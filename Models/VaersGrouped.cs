using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CSC365_Project3.Models
{
    public class VaersGrouped
    {
        public int VAERS_ID { get; set; }
        public string VAX_MANU { get; set; }
        public string RECVDATE { get; set; }
        public decimal? AGE_YRS { get; set; }
        public string SEX { get; set; }
        public string DIED { get; set; }
        public DateTime? DATEDIED { get; set; }
        public DateTime? VAX_DATE { get; set; }
        public int NumOfSymptoms { get { return Symptoms.Count; } }
        public List<string> Symptoms { get; set; } = [];
        public string AGE_BUCKET { get { return CollectionsOperations.ageBuckets.First(y => AGE_YRS >= y.MinAge && Math.Floor(AGE_YRS.Value) <= y.MaxAge).Label; } }
        public List<string> SymptomCats { get; set; } = [];
        public int? DaysDiedAfterVax
        {
            get
            {
                if (DATEDIED.HasValue && VAX_DATE.HasValue)
                {
                    return (DATEDIED.Value - VAX_DATE.Value).Days;
                }
                return null; // Return null if either date is not available
            }
        }
    }
}
