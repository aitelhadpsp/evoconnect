using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
    [Table("OBJET")]
    public class FileRecord
    {
        [Key]
        [Column("PK_OBJET")]
        public int PkObjet { get; set; }

        [Column("NOM")]
        [StringLength(500)]
        public string? Nom { get; set; }

        [Column("EXTENSION")]
        [StringLength(10)]
        public string? Extension { get; set; }

        [Required]
        [Column("ID_PATIENT")]
        public int IdPatient { get; set; }

        [Column("WIDTH")]
        public int? Width { get; set; }

        [Column("HEIGHT")]
        public int? Height { get; set; }

        [Column("TAILLE")]
        public int? Taille { get; set; }

        [Column("DATECREATION")]
        public DateTime? DateCreation { get; set; }

        [Column("FICHIER")]
        [StringLength(500)]
        public string? Fichier { get; set; }

        [Column("REP_STOCKAGE")]
        [StringLength(50)]
        public string? RepStockage { get; set; }

        [Column("DATEINSERTION")]
        public DateTime? DateInsertion { get; set; }
        public ICollection<ObjetEtiquette>? ObjetEtiquettes { get; set; }
        public ICollection<Etiquette>? Etiquettes { get; set; }
    }
}