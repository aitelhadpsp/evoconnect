using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("DENTALIS_ACTES_PATIENT")]
    public class DentalisActesPatient
    {
        [Key]
        [Column("AP_PK")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApPk { get; set; }

        [Column("AP_PATIENT")]
        [Required]
        public int ApPatient { get; set; } = -1;

        [Column("AP_ACTE")]
        [Required]
        public int ApActe { get; set; } = -1;

        [Column("AP_DATE_CREATION")]
        [Required]
        public DateTime ApDateCreation { get; set; } = new DateTime(2000, 1, 1);

        [Column("AP_DATE_MAJ")]
        [Required]
        public DateTime ApDateMaj { get; set; } = new DateTime(2000, 1, 1);

        [Column("AP_REALISE")]
        [Required]
        public short ApRealise { get; set; } = 0;

        [Column("AP_DATE_REALISE")]
        [Required]
        public DateTime ApDateRealise { get; set; } = new DateTime(2000, 1, 1);

        [Column("AP_DENT_FROM")]
        [Required]
        public short ApDentFrom { get; set; } = 0;

        [Column("AP_DENT_TO")]
        [Required]
        public short ApDentTo { get; set; } = 0;

        [Column("AP_COMM")]
        public byte[]? ApComm { get; set; }

        [Column("AP_DEVIS")]
        [Required]
        public int ApDevis { get; set; } = -1;

        [Column("AP_ID_ACTE_DEVIS")]
        [Required]
        public int ApIdActeDevis { get; set; } = -1;

        [Column("AP_PRAT")]
        [Required]
        public int ApPrat { get; set; } = -1;

        [Column("AP_LIBELLE_ACTE")]
        [MaxLength(255)]
        public string? ApLibelleActe { get; set; }

        [Column("AP_POSX")]
        public int? ApPosX { get; set; }

        [Column("AP_POSY")]
        public int? ApPosY { get; set; }

        [Column("AP_CODESECU")]
        [Required]
        public int ApCodeSecu { get; set; } = -1;

        [Column("SCRIPTE")]
        public byte[]? Scripte { get; set; }

        [Column("AP_MONTANT")]
        [Required]
        public float ApMontant { get; set; } = 0.0f;

        [Column("AP_SELECTED_DENTS")]
        [MaxLength(150)]
        public string? ApSelectedDents { get; set; }

        [Column("ID_CODESECU")]
        public int? IdCodeSecu { get; set; }

        [Column("AP_FACTURE")]
        [Required]
        [MaxLength(20)]
        public string ApFacture { get; set; } = string.Empty;

        [Column("AP_FACTURE_DATE")]
        public DateTime? ApFactureDate { get; set; }

        [Column("AP_MUTUELLE")]
        [Required]
        [MaxLength(20)]
        public string ApMutuelle { get; set; } = string.Empty;

        [Column("AP_MUTUELLE_DATE")]
        public DateTime? ApMutuelleDate { get; set; }

        [Column("AP_ARCHIVE")]
        [Required]
        public short ApArchive { get; set; } = 0;

        [Column("AP_FDENTAIRE")]
        [Required]
        [MaxLength(20)]
        public string ApFDentaire { get; set; } = string.Empty;

        [Column("AP_FDENTAIRE_DATE")]
        public DateTime? ApFDentaireDate { get; set; }

        [Column("AP_FACTURE_FDENTAIRE")]
        [MaxLength(50)]
        public string? ApFactureFDentaire { get; set; }

        [Column("AP_FINALISE")]
        [Required]
        public short ApFinalise { get; set; } = 0;

        [Column("AP_ID_COM_FINALISE")]
        [Required]
        public int ApIdComFinalise { get; set; } = 0;

        [Column("AP_DATE_FINALISE")]
        public DateTime? ApDateFinalise { get; set; }

        [Column("GUID")]
        public Guid? Guid { get; set; }

        // Navigation properties
        [ForeignKey("ApPatient")]
        public Patient? Patient { get; set; }

        [ForeignKey("ApActe")]
        public DentalisActes? Acte { get; set; }

        [ForeignKey("IdCodeSecu")]
        public CodeSecu? CodeSecu { get; set; }
    }
}