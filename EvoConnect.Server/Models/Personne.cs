using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    public class T_IDENTIFIANT
    {
        public static implicit operator int(T_IDENTIFIANT value) => value?.Value ?? 0;
        public static implicit operator T_IDENTIFIANT(int value) => new T_IDENTIFIANT { Value = value };
        public int Value { get; set; }
    }

    public class T_LIENPERS
    {
        public static implicit operator string(T_LIENPERS value) => value?.Value;
        public static implicit operator T_LIENPERS(string value) => new T_LIENPERS { Value = value };
        public string Value { get; set; }
    }

    [Table("PERSONNE")]
    public class Personne
    {
        [Key]
        [Column("ID_PERSONNE")]
        public int IdPersonne { get; set; }

        [Column("ID_ADRESSE")]
        public int? IdAdresse { get; set; }

        [Column("ID_UTIL")]
        public int? IdUtil { get; set; }

        [Column("ID_CAISSE")]
        public int? IdCaisse { get; set; }

        [Column("ADR_ID_ADRESSE")]
        public int? AdrIdAdresse { get; set; }

        [Required]
        [Column("PER_NOM")]
        [StringLength(30)]
        public string PerNom { get; set; }

        [Column("PER_NOMJF")]
        [StringLength(30)]
        public string? PerNomJf { get; set; }

        [Column("PER_PRENOM")]
        [StringLength(100)]
        public string? PerPrenom { get; set; }

        [Column("PER_GENRE")]
        [StringLength(1)]
        public string? PerGenre { get; set; }

        [Column("PER_SECU")]
        [StringLength(25)]
        public string? PerSecu { get; set; }

        [Required]
        [Column("PER_TYPE")]
        public short PerType { get; set; }

        [Column("PER_TELPRINC")]
        [StringLength(24)]
        public string? PerTelPrinc { get; set; }

        [Column("PER_TELTRAV1")]
        [StringLength(24)]
        public string? PerTelTrav1 { get; set; }

        [Column("PER_TELTRAV2")]
        [StringLength(24)]
        public string? PerTelTrav2 { get; set; }

        [Column("PER_TELECOPIE")]
        [StringLength(24)]
        public string? PerTelecopie { get; set; }

        [Column("PER_EMAIL")]
        [StringLength(128)]
        public string? PerEmail { get; set; }

        [Column("PER_RECEPTION")]
        [StringLength(24)]
        public string? PerReception { get; set; }

        [Column("PER_NOTES")]
        public byte[]? PerNotes { get; set; } = [];

        [Column("PER_POSTE")]
        [StringLength(10)]
        public string? PerPoste { get; set; }

        [Column("PCOM")]
        [StringLength(100)]
        public string? PCom { get; set; }

        [Column("PER_ADR1")]
        [StringLength(50)]
        public string? PerAdr1 { get; set; }

        [Column("PER_ADR2")]
        [StringLength(50)]
        public string? PerAdr2 { get; set; }

        [Column("PER_VILLE")]
        [StringLength(50)]
        public string? PerVille { get; set; }

        [Column("PER_CPOSTAL")]
        [StringLength(10)]
        public string? PerCPostal { get; set; }

        [Column("PROFESSION")]
        [StringLength(50)]
        public string? Profession { get; set; }

        [Column("MUTUELLE")]
        [StringLength(40)]
        public string? Mutuelle { get; set; }

        [Column("PER_DATNAISS")]
        public DateTime? PerDatNaiss { get; set; }

        [Column("POID")]
        public int? Poid { get; set; }

        [Column("EMAIL2")]
        [StringLength(100)]
        public string? Email2 { get; set; }

        [Column("GSM")]
        [StringLength(24)]
        public string? Gsm { get; set; }

        [Column("ICQ")]
        [StringLength(40)]
        public string? Icq { get; set; }

        [Column("IM1")]
        [StringLength(100)]
        public string? Im1 { get; set; }

        [Column("IM2")]
        [StringLength(100)]
        public string? Im2 { get; set; }

        [Column("LASTMODIF")]
        public DateTime? LastModif { get; set; }

        [Column("TELSUP0")]
        [StringLength(24)]
        public string? TelSup0 { get; set; }

        [Column("TELSUP3")]
        [StringLength(24)]
        public string? TelSup3 { get; set; }

        [Column("TELSUP4")]
        [StringLength(24)]
        public string? TelSup4 { get; set; }

        [Column("TELSUP5")]
        [StringLength(24)]
        public string? TelSup5 { get; set; }

        [Column("TELSUP6")]
        [StringLength(24)]
        public string? TelSup6 { get; set; }

        [Column("TELSUP8")]
        [StringLength(24)]
        public string? TelSup8 { get; set; }

        [Column("TELSUP10")]
        [StringLength(24)]
        public string? TelSup10 { get; set; }

        [Column("TELSUP11")]
        [StringLength(24)]
        public string? TelSup11 { get; set; }

        [Column("TELSUP12")]
        [StringLength(24)]
        public string? TelSup12 { get; set; }

        [Column("TELSUP13")]
        [StringLength(24)]
        public string? TelSup13 { get; set; }

        [Column("TELSUP14")]
        [StringLength(24)]
        public string? TelSup14 { get; set; }

        [Column("TELSUP15")]
        [StringLength(24)]
        public string? TelSup15 { get; set; }

        [Column("TELSUP16")]
        [StringLength(24)]
        public string? TelSup16 { get; set; }

        [Column("TELSUP17")]
        [StringLength(24)]
        public string? TelSup17 { get; set; }

        [Column("TELSUP18")]
        [StringLength(24)]
        public string? TelSup18 { get; set; }

        [Column("INDICETEL1")]
        public int? IndiceTel1 { get; set; }

        [Column("INDICETEL2")]
        public int? IndiceTel2 { get; set; }

        [Column("INDICETEL3")]
        public int? IndiceTel3 { get; set; }

        [Column("INDICETEL4")]
        public int? IndiceTel4 { get; set; }

        [Column("EMAIL3")]
        [StringLength(100)]
        public string? Email3 { get; set; }

        [Column("INDICEEMAIL")]
        public int? IndiceEmail { get; set; }

        [Column("INDICEADR")]
        public int? IndiceAdr { get; set; }

        [Column("PAYS_DOM")]
        [StringLength(100)]
        public string? PaysDom { get; set; }

        [Column("PERS_TITRE")]
        [StringLength(15)]
        public string? PersTitre { get; set; }

        [Column("PERS_SITEWEB")]
        [StringLength(255)]
        public string? PersSiteWeb { get; set; }

        [Column("OI_LOGIN")]
        [StringLength(60)]
        public string? OiLogin { get; set; }

        [Column("OI_MDP")]
        [StringLength(30)]
        public string? OiMdp { get; set; }

        [Column("OI_PROFIL")]
        public int? OiProfil { get; set; }

        [Column("OI_AUTORISATION")]
        public int? OiAutorisation { get; set; }

        [Column("PER_VILLE_NAISSANCE")]
        [StringLength(50)]
        public string? PerVilleNaissance { get; set; }

        [Column("PER_PAYS_NAISSANCE")]
        [StringLength(100)]
        public string? PerPaysNaissance { get; set; }

        [Column("PER_LANGUE_PARLE")]
        [StringLength(200)]
        public string? PerLangueParle { get; set; }

        [Column("PER_POPULATION_REF")]
        [StringLength(50)]
        public string? PerPopulationRef { get; set; }

        [Column("NOM_REP_IMAGE")]
        [StringLength(200)]
        public string? NomRepImage { get; set; }

        [Column("CATEGORIES")]
        public byte[]? Categories { get; set; } = [];

        [Column("POLITESSE")]
        [StringLength(100)]
        public string? Politesse { get; set; }

        [Column("APPELLATION")]
        [StringLength(100)]
        public string? Appellation { get; set; }

        [Column("TUVOUS")]
        public int? TuVous { get; set; }

        [Column("CHAMPBLOB1")]
        public byte[]? ChampBlob1 { get; set; } = [];

        [Column("INFOBLOB1")]
        public int? InfoBlob1 { get; set; }

        [Column("CHAMPBLOB2")]
        public byte[]? ChampBlob2 { get; set; } = [];

        [Column("INFOBLOB2")]
        public int? InfoBlob2 { get; set; }

        [Column("CHAMPBLOB3")]
        public byte[]? ChampBlob3 { get; set; } = [];

        [Column("INFOBLOB3")]
        public int? InfoBlob3 { get; set; }

        [Column("CHAMPBLOB4")]
        public byte[]? ChampBlob4 { get; set; } = [];

        [Column("INFOBLOB4")]
        public int? InfoBlob4 { get; set; }

        [Column("CHAMPBLOB5")]
        public byte[]? ChampBlob5 { get; set; } = [];

        [Column("INFOBLOB5")]
        public int? InfoBlob5 { get; set; }

        [Column("CHAMPBLOB6")]
        public byte[]? ChampBlob6 { get; set; } = [];

        [Column("INFOBLOB6")]
        public int? InfoBlob6 { get; set; }

        [Column("CHAMPBLOB7")]
        public byte[]? ChampBlob7 { get; set; } = [];

        [Column("INFOBLOB7")]
        public int? InfoBlob7 { get; set; }

        [Column("CHAMPBLOB8")]
        public byte[]? ChampBlob8 { get; set; } = [];

        [Column("INFOBLOB8")]
        public int? InfoBlob8 { get; set; }

        [Column("CHAMPBLOB9")]
        public byte[]? ChampBlob9 { get; set; } = [];

        [Column("INFOBLOB9")]
        public int? InfoBlob9 { get; set; }

        [Column("CHAMPBLOB10")]
        public byte[]? ChampBlob10 { get; set; } = [];

        [Column("INFOBLOB10")]
        public int? InfoBlob10 { get; set; }

        [Column("CHAMPBLOB11")]
        public byte[]? ChampBlob11 { get; set; } = [];

        [Column("INFOBLOB11")]
        public int? InfoBlob11 { get; set; }

        [Column("CHAMPBLOB12")]
        public byte[]? ChampBlob12 { get; set; } = [];

        [Column("INFOBLOB12")]
        public int? InfoBlob12 { get; set; }

        [Column("CHAMPBLOB13")]
        public byte[]? ChampBlob13 { get; set; } = [];

        [Column("INFOBLOB13")]
        public int? InfoBlob13 { get; set; }

        [Column("CHAMPBLOB14")]
        public byte[]? ChampBlob14 { get; set; } = [];

        [Column("INFOBLOB14")]
        public int? InfoBlob14 { get; set; }

        [Column("CHAMPBLOB15")]
        public byte[]? ChampBlob15 { get; set; } = [];

        [Column("INFOBLOB15")]
        public int? InfoBlob15 { get; set; }

        [Column("QUICKNOTE")]
        public byte[]? QuickNote { get; set; } = [];

        [Column("PER_NOTES_AUTRE")]
        public byte[]? PerNotesAutre { get; set; }

        [Column("LAST_RUM")]
        [StringLength(35)]
        public string? LastRum { get; set; }

        [Column("PER_POURCENTAGE")]
        public decimal? PerPourcentage { get; set; }

        [Column("GUID")]
        [StringLength(16)]
        public byte[]? Guid { get; set; } = [];

        // Navigation properties
        public ICollection<Patient> Patients { get; set; }
        public ICollection<RendezVous> RendezVousAsPersonne { get; set; }
        public ICollection<RendezVous> RendezVousAsProfessionnel { get; set; }
        public Utilisateur? Utilisateur { get; set; }

    }
}
