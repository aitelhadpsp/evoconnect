using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("RENDEZ_VOUS")]
    public class RendezVous
    {
        [Key]
        [Column("ID_RDV")]
        public int IdRdv { get; set; }

        [Required]
        [Column("ID_PERSONNE")]
        public int IdPersonne { get; set; }

        [Required]
        [Column("PER_ID_PERSONNE")]
        public int PerIdPersonne { get; set; }

        [Required]
        [Column("ID_ACTE")]
        public int IdActe { get; set; }

        [Required]
        [Column("ID_FAUTEUIL")]
        public int IdFauteuil { get; set; }

        [Required]
        [Column("RDV_DATE")]
        public DateTime RdvDate { get; set; }

        [Required]
        [Column("RDV_DUREE")]
        public int RdvDuree { get; set; }

        [Column("RDV_STATUT")]
        public int? RdvStatut { get; set; }

        [Column("RDV_ARRIVEE")]
        public DateTime? RdvArrivee { get; set; }

        [Column("RDV_COMM")]
        [StringLength(255)]
        public string? RdvComm { get; set; }

        [Column("RDV_QUAND")]
        public DateTime? RdvQuand { get; set; }

        [Column("LASTMODIF")]
        public DateTime? LastModif { get; set; }

        [Column("HEURE_FAUTEUIL")]
        public DateTime? HeureFauteuil { get; set; }

        [Column("HEURE_SALLEATTENTE")]
        public DateTime? HeureSalleAttente { get; set; }

        [Column("HEURE_SECRETARIAT")]
        public DateTime? HeureSecretariat { get; set; }

        [Column("HEURE_SORTI")]
        public DateTime? HeureSorti { get; set; }

        [Column("ISEXPORTED")]
        [StringLength(1)]
        public string IsExported { get; set; } = "N";

        [Column("FAUT_UTILISE")]
        public int? FautUtilise { get; set; }

        [Column("LOCALISATION")]
        public int? Localisation { get; set; }

        [Column("RDV_COMM_INTERNET")]
        [StringLength(255)]
        public string? RdvCommInternet { get; set; }

        [Column("RDV_FAMILLE")]
        public short RdvFamille { get; set; } = 0;

        [Column("RDV_REPERCUTION")]
        public int RdvRepercution { get; set; } = -1;

        [Column("DATE_LIVRAISON_STOCK")]
        public DateTime? DateLivraisonStock { get; set; }

        [Column("STATUT_CONFIRMATION")]
        [StringLength(100)]
        public string? StatutConfirmation { get; set; }

        [Column("GUID")]
        [StringLength(16)]
        public byte[]? Guid { get; set; }

     
        [ForeignKey("IdPersonne")]
        public Patient Patient { get; set; }

        [ForeignKey("PerIdPersonne")]
        public Personne Professionnel { get; set; }


        [ForeignKey("IdActe")]
        public Acte Acte { get; set; } 
    }
}
