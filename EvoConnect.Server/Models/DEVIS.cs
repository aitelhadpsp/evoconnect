using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
[Table("DEVIS")]
    public class Devis
    {
        [Key]
        [Column("DEVIS_PK")]
        public int DevisPk { get; set; }

        [Column("DEVIS_ID_PLAN")]
        public int DevisIdPlan { get; set; } = -1;

        [Column("DEVIS_ETAT")]
        public int DevisEtat { get; set; } = -1;

        [Column("DEVIS_DATE_DEMANDE")]
        public DateTime? DevisDateDemande { get; set; }

        [Column("DEVIS_DATE_ACCEPTATION")]
        public DateTime? DevisDateAcceptation { get; set; }

        [Column("DEVIS_DATE_REFUS")]
        public DateTime? DevisDateRefus { get; set; }

        [Column("DEVIS_DATE_MAJ")]
        public DateTime? DevisDateMaj { get; set; }

  /*       // Navigation properties
        [ForeignKey("DevisIdPlan")]
        public Plan? Plan { get; set; } */
    }
}