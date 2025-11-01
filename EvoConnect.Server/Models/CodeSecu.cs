using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("CODE_SECU")]
    public class CodeSecu
    {
        [Key]
        [Column("ID_CODESECU")]
        public int IdCodeSecu { get; set; }

        [Column("CODE_NOM")]
        [Required]
        [StringLength(50)]
        public string CodeNom { get; set; } = string.Empty;

        [Column("CODE_LIBELLE")]
        [Required]
        [StringLength(250)]
        public string CodeLibelle { get; set; } = string.Empty;

        [Column("CODE_VALEUR")]
        public double? CodeValeur { get; set; }

        [Column("CODE_DATE")]
        public DateTime? CodeDate { get; set; }

        [Column("CODE_STATUT")]
        public int? CodeStatut { get; set; }

        [Column("CODE_PRESTATION")]
        [StringLength(10)]
        public string? CodePrestation { get; set; }

        [Column("CODE_COEFF")]
        public double CodeCoeff { get; set; } = 0;

        [Column("CODE_APPAREIL")]
        [StringLength(17)]
        public string? CodeAppareil { get; set; }

        [Column("CODE_DECOMPVISIBLE")]
        public int? CodeDecompVisible { get; set; }

        [Column("CODE_DECOMPCOEFF")]
        [StringLength(50)]
        public string? CodeDecompCoeff { get; set; }

        [Column("CODE_VALEUR_EURO")]
        public double? CodeValeurEuro { get; set; }

        [Column("CODE_DEP")]
        public int? CodeDep { get; set; }

        [Column("TAUX_REMBOURS")]
        public float TauxRembours { get; set; } = 100;

        [Column("LOGICIEL")]
        public int Logiciel { get; set; } = 0;

        [Column("DUREEMOIS")]
        public int? DureeMois { get; set; }

        [Column("DUREEJOURS")]
        public int? DureeJours { get; set; }

        [Column("DUREEMOISFACT")]
        public int DureeMoisFact { get; set; } = 0;

        [Column("DUREEJOURSFACT")]
        public int DureeJoursFact { get; set; } = 0;

        [Column("CODE_TARIF_ENF")]
        public int CodeTarifEnf { get; set; } = 0;

        [Column("CODE_VALEUR_EURO_ENF")]
        public double CodeValeurEuroEnf { get; set; } = 0;

        [Column("CODE_COEFF_ENF")]
        public double CodeCoeffEnf { get; set; } = 0;

        [Column("CODE_CP_ENF")]
        [StringLength(10)]
        public string? CodeCpEnf { get; set; }

        [Column("CODE_USE_DEVIS")]
        [Required]
        public short CodeUseDevis { get; set; } = 1;

        [Column("CODE_SYMBOLE")]
        [Required]
        public int CodeSymbole { get; set; } = 0;

        [Column("CODE_COMPLEMENT")]
        [StringLength(100)]
        public string? CodeComplement { get; set; }

        [Column("CODE_VISU_CC")]
        [Required]
        public int CodeVisuCc { get; set; } = 1;

        [Column("CODE_CAISSES_LIEES")]
        public byte[]? CodeCaissesLiees { get; set; }

        [Column("FAMILLECS")]
        [Required]
        public int FamilleCs { get; set; } = -1;

        [Column("CODE_PR")]
        [Required]
        public float CodePr { get; set; } = 1;

        [Column("CODE_TVA")]
        [Required]
        public float CodeTva { get; set; } = 0;

        [Column("CODE_DESCR")]
        [StringLength(1000)]
        public string? CodeDescr { get; set; }

        [Column("TAUX_MAJOR")]
        public float? TauxMajor { get; set; } = 100;

        [Column("IS_ARCHIVE")]
        public int? IsArchive { get; set; } = 0;

        [Column("IS_HN")]
        [Required]
        public int IsHn { get; set; } = 0;

        [Column("CODE_PRES_PK")]
        public int? CodePresPk { get; set; }

        [Column("CODE_PRES_ENF_PK")]
        public int? CodePresEnfPk { get; set; }

        [Column("CODE_TARIF_BASE")]
        public double? CodeTarifBase { get; set; }

        [Column("MODIFICATEURS")]
        [StringLength(40)]
        public string? Modificateurs { get; set; }

        [Column("TYPE_CCAM")]
        public int TypeCcam { get; set; } = 0;

        [Column("TRANSPOSITION_CODE")]
        [StringLength(100)]
        public string TranspositionCode { get; set; } = string.Empty;

        [Column("TRANSPOSITION_CODE_NUM")]
        public int TranspositionCodeNum { get; set; } = 0;

        [Column("TRANSPOSITION_TARIF")]
        public double TranspositionTarif { get; set; } = 0;

        // Navigation property
        public ICollection<DentalisActes>? Actes { get; set; }
    }
}