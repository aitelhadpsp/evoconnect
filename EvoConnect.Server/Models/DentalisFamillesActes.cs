using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("DENTALIS_FAMILLES_ACTES")]
    public class DentalisFamillesActes
    {
        [Key]
        [Column("FACTE_PK")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FactePk { get; set; }

        [Column("FACTE_NOM")]
        [Required]
        [StringLength(255)]
        public string FacteNom { get; set; } = string.Empty;

        [Column("FACTE_COLOR")]
        public int? FacteColor { get; set; }

        [Column("FACTE_DATE_CREATION")]
        [Required]
        public DateTime FacteDateCreation { get; set; } = new DateTime(2000, 1, 1);

        [Column("FACTE_DATE_MAJ")]
        [Required]
        public DateTime FacteDateMaj { get; set; } = new DateTime(2000, 1, 1);

        [Column("FACTE_IMAGE")]
        [StringLength(255)]
        public string? FacteImage { get; set; }

        // Navigation property 
        public ICollection<DentalisActes>? Actes { get; set; }
    }
}