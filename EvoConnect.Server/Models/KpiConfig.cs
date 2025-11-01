using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("KPI_CONFIG")]
    public class KpiConfig
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("KPI_CODE")]
        public string KpiCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("KPI_NAME")]
        public string KpiName { get; set; }

        [MaxLength(10)]
        [Column("KPI_ICON")]
        public string? KpiIcon { get; set; }

        [Column("TARGET_VALUE", TypeName = "decimal(10,2)")]
        public decimal? TargetValue { get; set; }

        [MaxLength(20)]
        [Column("UNIT")]
        public string? Unit { get; set; }

        [Column("IS_ENABLED")]
        public short IsEnabled { get; set; } = 1;

        [Column("WARNING_THRESHOLD", TypeName = "decimal(10,2)")]
        public decimal? WarningThreshold { get; set; }

        [Column("CRITICAL_THRESHOLD", TypeName = "decimal(10,2)")]
        public decimal? CriticalThreshold { get; set; }

        [MaxLength(255)]
        [Column("DESCRIPTION")]
        public string? Description { get; set; }

        [Column("DISPLAY_ORDER")]
        public int? DisplayOrder { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Convenience property for boolean conversion
        [NotMapped]
        public bool IsActive
        {
            get => IsEnabled == 1;
            set => IsEnabled = (short)(value ? 1 : 0);
        }
    }
}