using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("PATIENT")]
    public class Patient
    {
        [Key]
        [Column("ID_PERSONNE")]
        public int IdPersonne { get; set; }

        [Column("PER_ID_PERSONNE")]
        public int? PerIdPersonne { get; set; }

        [Column("PER2_ID_PERSONNE")]
        public int? Per2IdPersonne { get; set; }

        [Column("PER3_ID_PERSONNE")]
        public int? Per3IdPersonne { get; set; }

        [Column("PER4_ID_PERSONNE")]
        public int? Per4IdPersonne { get; set; }

        [Column("PER5_ID_PERSONNE")]
        public int? Per5IdPersonne { get; set; }

        [Column("ID_STATUT")]
        public int? IdStatut { get; set; }

        [Required]
        [Column("PAT_NUMDOSSIER")]
        public int PatNumDossier { get; set; }

        [Column("PAT_DATECREATION")]
        public DateOnly? PatDateCreation { get; set; }

        [Column("PAT_CLASSDIAG")]
        [StringLength(4)]
        public string? PatClassDiag { get; set; }

        [Column("PAT_DIVISIONDIAG")]
        [StringLength(6)]
        public string? PatDivisionDiag { get; set; }

        [Column("PAT_DATERELANCE")]
        public DateTime? PatDateRelance { get; set; }

        [Column("PAT_REMARQUES")]
        [StringLength(255)]
        public string? PatRemarques { get; set; }

        [Column("PAT_ACCORD")]
        public DateTime? PatAccord { get; set; }

        [Column("LIEN_PAYEUR")]
        [StringLength(50)]
        public string? LienPayeur { get; set; }

        [Column("LIEN_ASSURE")]
        [StringLength(50)]
        public string? LienAssure { get; set; }

        [Column("PAT_CD")]
        public int? PatCd { get; set; }

        [Column("PAT_REFDOSSIER")]
        [StringLength(20)]
        public string? PatRefDossier { get; set; }

        [Column("DATE_RECO")]
        public DateTime? DateReco { get; set; }

        [Column("NUM_RECO")]
        public int? NumReco { get; set; }

        [Column("PCOM")]
        [StringLength(100)]
        public string? PCom { get; set; }

        [Column("TYDOSSIER")]
        [StringLength(10)]
        public string? TyDossier { get; set; }

        [Column("PHASE_TRAIT")]
        [StringLength(8)]
        public string? PhaseTrait { get; set; }

        [Column("PAT_DATERDV")]
        public DateTime? PatDateRdv { get; set; }

        [Column("PAT_SOLDE")]
        public decimal? PatSolde { get; set; }

        [Column("PAT_STATLASTRDV")]
        public int? PatStatLastRdv { get; set; }

        [Column("PAT_LASTRDV")]
        public DateTime? PatLastRdv { get; set; }

        [Column("PAT_DUREERDV")]
        public int? PatDureeRdv { get; set; }

        [Column("PAT_MSGAUDIO")]
        public int? PatMsgAudio { get; set; }

        [Column("REM_VISIBLE")]
        public int? RemVisible { get; set; }

        [Column("DEBUT_TRAIT")]
        public DateTime? DebutTrait { get; set; }

        [Column("PAT_SOLDE_EURO")]
        public float? PatSoldeEuro { get; set; }

        [Column("CMU")]
        public int? Cmu { get; set; }

        [Column("TIERSPAYANT")]
        public int? TiersPayant { get; set; }

        [Column("SECURITE")]
        public int? Securite { get; set; }

        [Column("ALLERGIE")]
        public byte[]? Allergie { get; set; }

        [Column("REP")]
        [StringLength(200)]
        public string? Rep { get; set; }

        [Column("LIBELLE_RDV")]
        [StringLength(100)]
        public string? LibelleRdv { get; set; }

        [Column("PAT_PREMIERRDV")]
        [StringLength(100)]
        public string? PatPremierRdv { get; set; }

        [Column("SEMESTRE_ENCOURS")]
        [StringLength(100)]
        public string? SemestreEncours { get; set; }

        [Column("DATEPROP")]
        [StringLength(100)]
        public string? DateProp { get; set; }

        [Column("DATEACCORD")]
        [StringLength(100)]
        public string? DateAccord { get; set; }

        [Column("NEXTPAIEMENT")]
        [StringLength(100)]
        public string? NextPaiement { get; set; }

        [Column("NEXTPAIEMENTEURO")]
        [StringLength(100)]
        public string? NextPaiementEuro { get; set; }

        [Column("DATENEXTPAIEMENT")]
        [StringLength(100)]
        public string? DateNextPaiement { get; set; }

        [Column("SEMESTRE_ENCOURS_DEP")]
        [StringLength(100)]
        public string? SemestreEncoursDep { get; set; }

        [Column("NOM_PLAN")]
        [StringLength(90)]
        public string? NomPlan { get; set; }

        [Column("ID_RADIO")]
        [StringLength(15)]
        public string? IdRadio { get; set; }

        [Column("MOULAGE")]
        public int? Moulage { get; set; }

        [Column("DERNIERTYPEPAIEMENT")]
        public int? DernierTypePaiement { get; set; }

        [Column("DATENEXTDEP")]
        [StringLength(20)]
        public string? DateNextDep { get; set; }

        [Column("SEMESTRE_NEXT_DEP")]
        [StringLength(50)]
        public string? SemestreNextDep { get; set; }

        [Column("PAT_REM_PLG")]
        [StringLength(255)]
        public string? PatRemPlg { get; set; }

        [Column("LASTRDV_HEURE_ARRIVE")]
        public DateTime? LastRdvHeureArrive { get; set; }

        [Column("PAT_OPEN_BNOTES")]
        public int PatOpenBNotes { get; set; } = 0;

        [Column("PAT_REPRISE_TT")]
        public DateTime? PatRepriseTt { get; set; }

        [Column("PAT_NUM")]
        public int PatNum { get; set; } = -1;

        [Column("COMM_NEXT_RDV")]
        [StringLength(255)]
        public string? CommNextRdv { get; set; }

        [Column("PAT_COLOR")]
        public int PatColor { get; set; } = -1;

        [Column("SHOW_OH_PHOTO")]
        public short ShowOhPhoto { get; set; } = 1;

        [Column("BANQUE_PAT")]
        [StringLength(100)]
        public string BanquePat { get; set; } = "-1";

        [Column("PAT_DUREE_TT")]
        public int PatDureeTt { get; set; } = 0;

        [Column("PAT_SOLDE_ONLY")]
        public float? PatSoldeOnly { get; set; }

        [Column("CAISSE_SOLDE")]
        public float? CaisseSolde { get; set; }

        [Column("MUTUELLE_SOLDE")]
        public float? MutuelleSolde { get; set; }

        [Column("FAMILLE_SOLDE")]
        public float? FamilleSolde { get; set; }

        [Column("ACOMPTE_SOLDE")]
        public float? AcompteSolde { get; set; }

        [Column("REGL_ANTICIPE")]
        public float? ReglAnticipe { get; set; }

        [Column("NB_JOURS_SOLDE")]
        public int? NbJoursSolde { get; set; }

        [Column("AUTH_MAIL")]
        public int AuthMail { get; set; } = 1;

        [Column("ALD")]
        public int Ald { get; set; } = 0;

        [Column("DATE_HOSPITAL_ENTREE")]
        public DateTime? DateHospitalEntree { get; set; }

        [Column("DATE_HOSPITAL_SORTIE")]
        public DateTime? DateHospitalSortie { get; set; }

        [Column("REMARQUE_HOSPITAL")]
        [StringLength(255)]
        public string? RemarqueHospital { get; set; }

        [ForeignKey("IdPersonne")]
        public Personne Personne { get; set; }

        [ForeignKey("PerIdPersonne")]
        public Personne? ProfessionnelPersonne { get; set; }

        [ForeignKey("Per2IdPersonne")]
        public Personne? Professionnel2Personne { get; set; }

        [ForeignKey("Per3IdPersonne")]
        public Personne? Professionnel3Personne { get; set; }

        [ForeignKey("Per4IdPersonne")]
        public Personne? Professionnel4Personne { get; set; }

        [ForeignKey("Per5IdPersonne")]
        public Personne? Professionnel5Personne { get; set; }

        public ICollection<RendezVous> RendezVous { get; set; }
  
        public ICollection<DentalisDevis>? DentalisDevis { get; set; }
        public ICollection<ZoneComm>? ZoneComm { get; set; }
        public ICollection<DentalisActesPatient>? DentalisActes { get; set; }

    }
}