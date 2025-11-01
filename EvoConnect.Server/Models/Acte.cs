using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("ACTES")]
    public class Acte
    {
        [Key]
        [Column("ID_ACTE")]
        public int IdActe { get; set; }

        [Required]
        [Column("ACTE_LIBELLE", TypeName = "CHAR(20)")]
        [StringLength(20)]
        public string ActeLibelle { get; set; }

        [Required]
        [Column("ACTE_DURESTD")]
        public int ActeDurestd { get; set; }

        [Required]
        [Column("ACTE_COULEUR")]
        public int ActeCouleur { get; set; }

        [Column("TYPE_ACTE")]
        public int? TypeActe { get; set; }

        [Column("NB_FAUTBLOC")]
        public float? NbFautBloc { get; set; }

        [Column("CODE_PLANING", TypeName = "VARCHAR(10)")]
        [StringLength(10)]
        public string? CodePlaning { get; set; }

        [Column("TEMPS_CHRONO")]
        public int? TempsChrono { get; set; }

        [Required]
        [Column("ANTICIP_RDV")]
        public int AnticipRdv { get; set; } = 0;

        [Required]
        [Column("NO_PRINT")]
        public short NoPrint { get; set; } = 0;

        [Required]
        [Column("SMS")]
        public int Sms { get; set; } = 0;

        [Required]
        [Column("COURRIER")]
        public int Courrier { get; set; } = -1;

        [Required]
        [Column("ANTICIPATION_SMS")]
        public int AnticipationSms { get; set; } = 0;

        [Required]
        [Column("CATEG_STATS")]
        public int CategStats { get; set; } = -1;

        [Required]
        [Column("TARIF")]
        public float Tarif { get; set; } = 0.0f;
        public ICollection<RendezVous> RendezVous { get; set; } = new List<RendezVous>();
    }
}
