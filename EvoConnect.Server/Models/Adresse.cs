using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
   [Table("CAISSE")]
    public class Caisse
    {
        [Key]
        [Column("ID_CAISSE")]
        public int IdCaisse { get; set; }

        [Column("ID_ADRESSE")]
        public int? IdAdresse { get; set; }

        [Column("CAISSE_NOM")]
        [StringLength(30)]
        public string? CaisseNom { get; set; }

        [Column("CAISSE_TEL")]
        [StringLength(24)] 
        public string? CaisseTel { get; set; }

        [Column("CAISSE_DEVIS")]
        [StringLength(100)]
        public string? CaisseDevis { get; set; }

        // Navigation properties
        public ICollection<PlanApplique>? PlansAppliques { get; set; }
    }
}