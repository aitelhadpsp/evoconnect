using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
    [Table("OBJETS_ETIQUETTE")]
    public class ObjetEtiquette
    {
        [Key]
        [Column("ID_PK")]
        public int IdPk { get; set; }

        [Required]
        [Column("ID_ETIQUETTE")]
        public int IdEtiquette { get; set; }

        [Required]
        [Column("ID_OBJET")]
        public int IdObjet { get; set; }

        // Navigation properties
        [ForeignKey("IdEtiquette")]
        public Etiquette? Etiquette { get; set; }

        [ForeignKey("IdObjet")]
        public FileRecord? Objet { get; set; }
    }
}