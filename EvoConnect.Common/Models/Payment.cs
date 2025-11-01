using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Common.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime MadeAt { get; set; }
        public float Amount { get; set; }
        public int PaymentMethode { get; set; }
    }
}