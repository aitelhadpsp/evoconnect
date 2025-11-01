using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Common.Models
{
    public class Treatment
    {
        public int InternId { get; set; }
        public int PatientId { get; set; }
        public string Label { get; set; }
        public decimal Amount { get; set; }
        public decimal Paid { get; set; }
        public DateTime? RealisedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsRealised { get; set; }
        public string Teeths { get; set; }
    }
}