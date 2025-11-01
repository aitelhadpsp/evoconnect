using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
    [Table("DENTALIS_DEVIS")]
    public class DentalisDevis
    {
        [Key]
        [Column("DEVIS_PK")]
        public int DevisPk { get; set; }

        [Required]
        [Column("DEVIS_NOM")]
        [StringLength(150)]
        public string DevisNom { get; set; } = "";

        [Required]
        [Column("DEVIS_ID_PATIENT")]
        public int DevisIdPatient { get; set; } = -1;

        [Column("DEVIS_ACCEPTE")]
        public short DevisAccepte { get; set; } = 0;

        [Column("DEVIS_MONTANT")]
        public float DevisMontant { get; set; } = 0.0f;

        [Required]
        [Column("DEVIS_DATE_CREAT")]
        public DateTime DevisDateCreat { get; set; } = new DateTime(2000, 1, 1);

        [Required]
        [Column("DEVIS_DATE_MAJ")]
        public DateTime DevisDateMaj { get; set; } = new DateTime(2000, 1, 1);

        [Required]
        [Column("DEVIS_DATE_IMPR")]
        public DateTime DevisDateImpr { get; set; } = new DateTime(2000, 1, 1);

        [Column("DEVIS_PRESENTE")]
        public short DevisPresente { get; set; } = 0;

        [Required]
        [Column("DEVIS_DATE_PRESENT")]
        public DateTime DevisDatePresent { get; set; } = new DateTime(2000, 1, 1);

        [Column("DEVIS_NUM_FACTURE")]
        [StringLength(50)]
        public string? DevisNumFacture { get; set; }

        [Column("DEVIS_MODE_REG")]
        [StringLength(20)]
        public string? DevisModeReg { get; set; }

        [Column("DEVIS_REMISE")]
        public float? DevisRemise { get; set; } = 0.0f;

        [Column("DEVIS_MONTANT_AV_REMISE")]
        public float? DevisMontantAvRemise { get; set; } = 0.0f;

        [Column("DEVIS_REMISE_TYPE")]
        public short DevisRemiseType { get; set; } = 0;

        // Navigation properties
        [ForeignKey("DevisIdPatient")]
        public Patient? Patient { get; set; }

        // Propriétés calculées
        [NotMapped]
        public bool IsAccepted => DevisAccepte == 1;

        [NotMapped]
        public bool IsPresented => DevisPresente == 1;

        [NotMapped]
        public float MontantFinal => DevisMontant - (DevisRemise ?? 0);

        [NotMapped]
        public string StatusText => IsAccepted ? "Accepté" : IsPresented ? "Présenté" : "Brouillon";
    }
}