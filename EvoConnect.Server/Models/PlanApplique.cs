using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
    [Table("PLAN_APPLIQUE")]
    public class PlanApplique
    {
        // Clés composites - pas de [Key] car pas de clé primaire unique définie
        [Required]
        [Column("ID_PATIENT")]
        public int IdPatient { get; set; }

        [Column("DATE_RENS")]
        public DateTime? DateRens { get; set; }

        [Column("DATE_REF_PAIE")]
        public DateTime? DateRefPaie { get; set; }

        [Required]
        [Column("TYPE_REGLE")]
        public int TypeRegle { get; set; }

        [Column("LIBELLE")]
        [StringLength(200)]
        public string? Libelle { get; set; }

        [Column("MONTANT")]
        public float? Montant { get; set; }

        [Required]
        [Column("CODE_PLAN")]
        public int CodePlan { get; set; }

        [Required]
        [Column("NUM_DATE")]
        public int NumDate { get; set; }

        [Column("DATE_REF")]
        public int? DateRef { get; set; }

        [Column("NB_JOURS")]
        public int? NbJours { get; set; }

        [Column("NB_MOIS")]
        public int? NbMois { get; set; }

        [Column("TARIF")]
        [StringLength(200)]
        public string? Tarif { get; set; }

        [Column("FLAG_PRODUIT")]
        public int? FlagProduit { get; set; }

        [Column("MODE_PAY")]
        public int? ModePay { get; set; }

        [Column("MONTANT_ENCAIS")]
        public float? MontantEncais { get; set; }

        [Column("FLAG_BANQUE")]
        [StringLength(2)]
        public string? FlagBanque { get; set; }

        [Column("BANQUE")]
        [StringLength(70)]
        public string? Banque { get; set; }

        [Column("REMARQUE")]
        [StringLength(100)]
        public string? Remarque { get; set; }

        [Column("NUMFSE")]
        public int NumFse { get; set; } = 0;

        [Column("NUMLIGFSE")]
        public int NumLigFse { get; set; } = 0;

        [Column("ID_CODESECU")]
        public int IdCodeSecu { get; set; } = 0;

        [Column("ID_PLAN")]
        public int? IdPlan { get; set; }

        [Column("DATE_PROP")]
        public DateTime? DateProp { get; set; }

        [Column("QUALIFDEPENSE")]
        public int? QualifDepense { get; set; }

        [Column("ALD")]
        public int? Ald { get; set; }

        [Column("NBKILOMETRE")]
        public int? NbKilometre { get; set; }

        [Column("RMO")]
        public int? Rmo { get; set; }

        [Column("LIEUEXECUTION")]
        public int? LieuExecution { get; set; }

        [Column("CODEACCENTENTE")]
        public int? CodeAccentente { get; set; }

        [Column("DATEPOSTAGE")]
        public DateTime? DatePostage { get; set; }

        [Column("IDENTENTE")]
        [StringLength(15)]
        public string? IdEntente { get; set; }

        [Column("ID_PRATICIEN")]
        public int? IdPraticien { get; set; }

        [Column("SEMESTRE")]
        [StringLength(50)]
        public string? Semestre { get; set; }

        [Column("MONTANT_EURO")]
        public float? MontantEuro { get; set; }

        [Column("MONTANT_ENCAIS_EURO")]
        public float? MontantEncaisEuro { get; set; }

        [Column("ENREG_CREATE_WITHDEVISE")]
        [StringLength(5)]
        public string? EnregCreateWithDevise { get; set; }

        [Column("DATE_REMBANQUE")]
        public DateTime? DateRemBanque { get; set; }

        [Column("EXONERATION")]
        public int? Exoneration { get; set; }

        [Column("ACCIDENT")]
        public int? Accident { get; set; }

        [Column("DIMANCHE_FERIE")]
        public int? DimancheFerie { get; set; }

        [Column("NUIT")]
        public int? Nuit { get; set; }

        [Column("NUMDENT")]
        public int? NumDent { get; set; }

        [Column("DATEACCIDENT")]
        public DateTime? DateAccident { get; set; }

        [Column("FSE_FORMATE")]
        public int? FseFormate { get; set; }

        [Column("MONTANT_TIERSPAYANT")]
        public float? MontantTiersPayant { get; set; }

        [Column("MONTANT_MUTUELLE")]
        public float? MontantMutuelle { get; set; }

        [Column("ID_MUTUELLE")]
        public int? IdMutuelle { get; set; }

        [Column("ID_CAISSE")]
        public int? IdCaisse { get; set; }

        [Column("FLAG_MUTUELLE")]
        [StringLength(2)]
        public string? FlagMutuelle { get; set; }

        [Column("INFOSECHEANCES")]
        public int? InfoSecheances { get; set; }

        [Column("VALEURED")]
        public float? ValeurEd { get; set; }

        [Column("TYPEPAYEUR")]
        public int? TypePayeur { get; set; }

        [Column("DONNEELE")]
        public DateTime? DonneeLeDate { get; set; }

        [Column("DEP")]
        public int? Dep { get; set; }

        [Column("DATEIMPRESSION")]
        public DateTime? DateImpression { get; set; }

        [Column("NUM_CHEQUE")]
        [StringLength(20)]
        public string? NumCheque { get; set; }

        [Column("NOM_BANQUE")]
        [StringLength(70)]
        public string? NomBanque { get; set; }

        [Column("MULTIPAIE")]
        public int? MultiPaie { get; set; }

        [Column("PASFAIREDEP")]
        public int? PasFaireDep { get; set; }

        [Column("INFOSIMPAYE")]
        public int? InfosImPaye { get; set; }

        [Column("DUREEM")]
        public int? DureeM { get; set; }

        [Column("DUREEJ")]
        public int? DureeJ { get; set; }

        [Column("NUMERO_FSE")]
        public int? NumeroFse { get; set; }

        [Column("DATE_FSE")]
        public DateTime? DateFse { get; set; }

        [Column("DENTS")]
        [StringLength(100)]
        public string? Dents { get; set; }

        [Column("MODE_FSE")]
        [StringLength(20)]
        public string? ModeFse { get; set; }

        [Column("PA_LAST_FRF")]
        [StringLength(50)]
        public string? PaLastFrf { get; set; }

        [Column("PA_DATE_LAST_FRF")]
        public DateTime? PaDateLastFrf { get; set; }

        [Column("PA_CPT_FRF")]
        public int PaCptFrf { get; set; } = 0;

        [Column("NUMERO_MH")]
        public int NumeroMh { get; set; } = -1;

        [Column("NUMERO_DEVIS")]
        public int NumeroDevis { get; set; } = -1;

        [Column("PA_DATE_FIRST_FRF")]
        public DateTime? PaDateFirstFrf { get; set; }

        [Column("ACTE_ENFANTADULTE")]
        public short ActeEnfantAdulte { get; set; } = 0;

        [Column("DATEDEPPREVUE")]
        public DateTime? DateDepPrevue { get; set; }

        [Column("INDICPARCOURSSOIN")]
        [StringLength(10)]
        public string? IndicParcoursSoin { get; set; }

        [Column("ID_MULTI_PAIEMENT")]
        public int? IdMultiPaiement { get; set; }

        [Column("TRANSPOSITION_CODE")]
        [StringLength(100)]
        public string TranspositionCode { get; set; } = "";

        [Column("TRANSPOSITION_TARIF")]
        public float TranspositionTarif { get; set; } = 0;

        [Column("TRANSPOSITION_CODE_NUM")]
        public int TranspositionCodeNum { get; set; } = 0;

        [Column("NUMERO_RUM")]
        [StringLength(35)]
        public string? NumeroRum { get; set; }

        [Column("SOINS_13ANS")]
        public int Soins13Ans { get; set; } = 0;

        [Column("RADIO_5ANS")]
        public int Radio5Ans { get; set; } = 0;

        [Column("CODE_ASSOS_CCAM")]
        [StringLength(4)]
        public string? CodeAssosCcam { get; set; }

        [Column("LOG_USER")]
        public int LogUser { get; set; } = 0;

        [Column("GUID")]
        [StringLength(16)]
        public byte[]? Guid { get; set; }

        // Navigation properties
        [ForeignKey("IdPatient")]
        public Patient? Patient { get; set; }

  /*       [ForeignKey("IdPlan")]
        public Plan? Plan { get; set; }
 */
        [ForeignKey("IdPraticien")]
        public Personne? Praticien { get; set; }

        [ForeignKey("IdMutuelle")]
        public Mutuelle? Mutuelle { get; set; }

        [ForeignKey("IdCaisse")]
        public Caisse? Caisse { get; set; }
    }
}