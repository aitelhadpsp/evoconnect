using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
       [Table("ETIQUETTE")]
    public class Etiquette
    {
        [Key]
        [Column("PK_ETIQUETTE")]
        public int PkEtiquette { get; set; }

        [Column("NOM")]
        [StringLength(250)]
        public string? Nom { get; set; }

        [Column("TYPE_ETIQUETTE")]
        public int? TypeEtiquette { get; set; }

        [Column("REF_FAMILLE")]
        public int? RefFamille { get; set; }

        [Column("ETIQUETTE_IMAGE")]
        [StringLength(255)]
        public string? EtiquetteImage { get; set; }

        [Column("WORD_ID")]
        public int WordId { get; set; } = 0;

        // Navigation properties
        // Relation Many-to-Many avec Objet via ObjetEtiquette
        public ICollection<ObjetEtiquette>? ObjetEtiquettes { get; set; }
        public ICollection<FileRecord>? Objets { get; set; }
    }
}