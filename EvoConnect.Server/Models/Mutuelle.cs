using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
     [Table("MUTUELLE")]
    public class Mutuelle
    {
        [Key]
        [Column("ID_MUTUELLE")]
        public int IdMutuelle { get; set; }

        [Column("ID_ADRESSE")]
        public int? IdAdresse { get; set; }

        [Column("MUTUELLE_NOM")]
        [StringLength(100)]
        public string? MutuelleNom { get; set; }

        [Column("MUTUELLE_TEL")]
        [StringLength(24)] // Taille typique pour T_TELNUM
        public string? MutuelleTel { get; set; }

        [Column("MUTUELLE_NUMERO")]
        [StringLength(20)]
        public string? MutuelleNumero { get; set; }

        // Navigation properties
        public ICollection<PlanApplique>? PlansAppliques { get; set; }
    }
}