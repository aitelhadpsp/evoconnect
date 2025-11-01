using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("DENTALIS_ACTES")]
    public class DentalisActes
    {
        [Key]
        [Column("ACTE_PK")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActePk { get; set; }

        [Column("ACTE_NOM")]
        public string ActeNom { get; set; } = string.Empty;

        [Column("ACTE_FAMILLE")]
        public int ActeFamille { get; set; } = 0;

        [Column("ACTE_DATE_CREATION")]
        public DateTime ActeDateCreation { get; set; } = new DateTime(2000, 1, 1);

        [Column("ACTE_DATE_MAJ")]
        public DateTime ActeDateMaj { get; set; } = new DateTime(2000, 1, 1);

        [Column("ACTE_IMAGE")]
        public string? ActeImage { get; set; }

        [Column("ACTE_ABREGE")]
        [StringLength(50)]
        public string? ActeAbrege { get; set; }

        [Column("ACTE_CODE_SECU")]
        public int ActeCodeSecu { get; set; } = -1;

        [Column("ACTE_NB_DENTS")]
        public short ActeNbDents { get; set; } = 0;

        [Column("ACTE_CATEGORIE")]
        public int ActeCategorie { get; set; } = -1;

        [Column("ACTE_POINTEUR")]
        public short? ActePointeur { get; set; } = 0;

        [Column("ACTE_COLOR_POINTEUR")]
        public int? ActeColorPointeur { get; set; } = 0;

        [Column("ACTE_MONTANT_A")]
        public float? ActeMontantA { get; set; }

        [Column("ACTE_MONTANT_E")]
        public float? ActeMontantE { get; set; }

        [Column("ACTE_QUALIF_DEP")]
        public int ActeQualifDep { get; set; } = 0;

        [Column("ID_GRAPHIQUE")]
        public int? IdGraphique { get; set; }

        [Column("GRAPHIQUE_COLOR")]
        public int? GraphiqueColor { get; set; }

        [Column("SCRIPT_VB")]
        public string? ScriptVb { get; set; }

        [Column("AA_SCRIPTE_PEVISUALISATION")]
        public byte[]? AaScriptePevisualisation { get; set; }

        [Column("AA_USE_PEVISUALISATION")]
        public int? AaUsePevisualisation { get; set; } = 0;

        [Column("TRANSPOSITION_CODE")]
        public string TranspositionCode { get; set; } = string.Empty;

        [Column("TRANSPOSITION_TARIF")]
        public double TranspositionTarif { get; set; } = 0;

        [Column("TRANSPOSITION_CODE_NUM")]
        public int TranspositionCodeNum { get; set; } = 0;

        [Column("ACTE_MONTANT_U")]
        public float? ActeMontantU { get; set; }

        // Navigation properties
        [ForeignKey("ActeFamille")]
        public DentalisFamillesActes? Famille { get; set; }

        [ForeignKey("ActeCodeSecu")]
        public CodeSecu? CodeSecu { get; set; }

        public ICollection<DentalisActesPatient>? ActesPatient { get; set; }
    }
}