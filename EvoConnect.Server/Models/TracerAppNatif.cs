using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("TRACER_APP_NATIF")]
    public class TracerAppNatif
    {
        [Key]
        [Column("ID_KEY")]
        public int IdKey { get; set; }

        [Required]
        [Column("ID_ITEM")]
        public int IdItem { get; set; }

        [Required]
        [Column("TABLE_NAME")]
        [StringLength(20)]
        public string TableName { get; set; } = string.Empty;

        [Column("DATE_ITEM")]
        public DateTime? DateItem { get; set; }
    }
}
