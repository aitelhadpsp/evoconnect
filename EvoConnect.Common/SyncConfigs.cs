using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoConnect.Common
{
    public class SyncConfigs
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }
        public bool FlashEvo { get; set; } = false;
        public bool StartUp {get;set;}= false;
        public DateTime? AppointmentsSync { get; set; }
        public DateTime? TreatmentsSync { get; set; }
        public DateTime? PaymentsSync { get; set; }
        public DateTime? PatientsSync { get; set; }
        public DateTime? NotesSync { get; set; }
        public DateTime? DoctorsSync { get; set; }
        public DateTime? LabelsSync { get; set; }
        public DateTime? ImageSync { get; set; }
        public int? ImageLabelsSync { get; set; }
        public int? DeleteSync { get; set; }

    }
}
