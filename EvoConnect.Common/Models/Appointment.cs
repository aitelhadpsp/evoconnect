using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Common.Models
{
    public class Appointment
    {
        public int ID_RDV { get; set; }
        public DateTime RDV_DATE { get; set; }
        public int ID_PERSONNE { get; set; }
        public int ID_ACTE { get; set; }
        public string PER_NOM { get; set; }
        public string PER_PRENOM { get; set; }
        public string PER_GENRE { get; set; }
        public string ACTE_LIBELLE { get; set; }
        public string UTIL_IDENT { get; set; }
        public string PER_TELPRINC { get; set; }
        public string ACTE_COULEUR { get; set; }
        public int ID_UTIL { get; set; }
        public string GUID { get; set; }
    }
}