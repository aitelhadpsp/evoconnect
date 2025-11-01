using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("UTILISATEUR")]
    public class Utilisateur
    {
        [Key]
        [Column("ID_UTIL")]
        public int IdUtil { get; set; }

        [Column("ID_PERSONNE")]
        public int? IdPersonne { get; set; }

        [Column("UTIL_IDENT")]
        [StringLength(15)]
        public string? Identifiant { get; set; }

        [Column("UTIL_PWD")]
        [StringLength(15)]
        public string? MotDePasse { get; set; }

        [Column("UTIL_CONNECTE")]
        public short? Connecte { get; set; }

        [Column("UTIL_LASTCONNECT")]
        public DateTime? DerniereConnexion { get; set; }

        [Column("UTIL_TYPE")]
        public short? Type { get; set; }

        [Column("UTIL_DROITS")]
        [StringLength(10)]
        public string? Droits { get; set; }

        [Column("UTIL_GESTION")]
        [StringLength(10)]
        public string? Gestion { get; set; }

        [ForeignKey("IdPersonne")]
        public Personne? Personne { get; set; }
    }
}
