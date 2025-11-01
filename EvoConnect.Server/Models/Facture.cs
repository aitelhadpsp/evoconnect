using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
 [Table("FACTURE")]
    public class Facture
    {
        [Key]
        [Column("ID_FACTURE")]
        [StringLength(20)]
        public string IdFacture { get; set; }

        [Required]
        [Column("ID_PERSONNE")]
        public int IdPersonne { get; set; }

        [Required]
        [Column("ID_UTIL")]
        public int IdUtil { get; set; }

        [Column("FACT_MONTANT")]
        public float? FactMontant { get; set; }

        [Required]
        [Column("FACT_DATE")]
        public DateTime FactDate { get; set; }

        [Column("FACT_PART_PATIENT")]
        public float FactPartPatient { get; set; } = 0.0f;

        // Navigation properties
        [ForeignKey("IdPersonne")]
        public Personne? Personne { get; set; }

        [ForeignKey("IdUtil")]
        public Personne? Utilisateur { get; set; }
    }
}