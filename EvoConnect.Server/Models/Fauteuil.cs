using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
   [Table("FAUTEUIL")]
    public class Fauteuil
    {
        [Key]
        [Column("ID_FAUTEUIL")]
        public int IdFauteuil { get; set; }

        [Required]
        [Column("FAUT_LIBELLE")]
        [StringLength(30)] 
        public string FautLibelle { get; set; }

        [Column("FAUT_PRATICIEN")]
        public int? FautPraticien { get; set; } = -1;

        public ICollection<RendezVous>? RendezVous { get; set; }
    }
}