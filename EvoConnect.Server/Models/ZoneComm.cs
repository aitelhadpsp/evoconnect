using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvoConnect.Server.Models
{
    [Table("ZONE_COMM")]
    public class ZoneComm
    {
        [Key]
        [Column("ID_COMM")]
        public int IdComm { get; set; }

        [Required]
        [Column("ID_PAT")]
        public int IdPatient { get; set; }

        [Column("DATE_COMM")]
        public DateTime? DateComm { get; set; }

        [Column("CODE_HYG")]
        [StringLength(10)]
        public string? CodeHyg { get; set; }

        [Column("CODE_LIBRE")]
        [StringLength(10)]
        public string? CodeLibre { get; set; }

        [Column("NOM_DOC")]
        [StringLength(50)]
        public string? NomDoc { get; set; }

        [Column("COMMENTAIRE")]
        [StringLength(160)]
        public string? Commentaire { get; set; }

        [Column("IS_VOCAL")]
        public int? IsVocal { get; set; }

        [Column("ID_VOCAL")]
        public int? IdVocal { get; set; }

        [Column("CHECKED")]
        public int? Checked { get; set; }

        [Column("COULEUR_ELCOMMENTAIRE")]
        public int? CouleurElCommentaire { get; set; }

        [Column("POLICE_ELCOMMENTAIRE")]
        [StringLength(100)]
        public string? PoliceElCommentaire { get; set; }

        [Column("TAILLE_ELCOMMENTAIRE")]
        public int? TailleElCommentaire { get; set; }

        [Column("BOLD_ELCOMMENTAIRE")]
        public int? BoldElCommentaire { get; set; }

        [Column("ITALIC_ELCOMMENTAIRE")]
        public int? ItalicElCommentaire { get; set; }

        [Column("UNDERLINE_ELCOMMENTAIRE")]
        public int? UnderlineElCommentaire { get; set; }

        [Column("STRIKEOUT_ELCOMMENTAIRE")]
        public int? StrikeoutElCommentaire { get; set; }

        [Column("COMM_RDV")]
        public int? CommRdv { get; set; }

        [Column("COMM_RDV_DATE")]
        public DateTime? CommRdvDate { get; set; }

        [Column("COMM_RDV_ACTE")]
        public int? CommRdvActe { get; set; }

        [Column("COMMTYPE_DATE")]
        public int? CommTypeDate { get; set; }

        [Column("COMM_RDV_FAUTEUIL")]
        public int? CommRdvFauteuil { get; set; }

        [Column("FAIT")]
        public string? Fait { get; set; }

        [Column("AFAIRE")]
        public string? Afaire { get; set; }

        [Column("COMM_PRAT")]
        public int? CommPrat { get; set; }

        [Column("COMM_RDV_DATE_MAX")]
        public DateTime? CommRdvDateMax { get; set; }

        [Column("COMM_RDV_SEMAINE_MIN")]
        public short? CommRdvSemaineMin { get; set; } = -1;

        [Column("COMM_RDV_SEMAINE_MAX")]
        public short? CommRdvSemaineMax { get; set; } = -1;

        [Column("COM1")]
        public string? Com1 { get; set; }

        [Column("COM2")]
        public string? Com2 { get; set; }

        [Column("COM3")]
        public string? Com3 { get; set; }

        [Column("COM4")]
        public string? Com4 { get; set; }

        [Column("COM5")]
        public string? Com5 { get; set; }

        [Column("COM6")]
        public string? Com6 { get; set; }

        [Column("COM7")]
        public string? Com7 { get; set; }

        [Column("COM8")]
        public string? Com8 { get; set; }

        [Column("COLSUP1")]
        public string? Colsup1 { get; set; }

        [Column("COLSUP2")]
        public string? Colsup2 { get; set; }

        [Column("COLSUP3")]
        public string? Colsup3 { get; set; }

        [Column("COLSUP4")]
        public string? Colsup4 { get; set; }

        [Column("COLSUP5")]
        public string? Colsup5 { get; set; }

        [Column("COLSUP6")]
        public string? Colsup6 { get; set; }

        [Column("COLSUP7")]
        public string? Colsup7 { get; set; }

        [Column("COLSUP8")]
        public string? Colsup8 { get; set; }

        [Column("COMM_RDV_DUREE")]
        public int? CommRdvDuree { get; set; }

        [Column("ZC_LOGICIEL")]
        public int ZcLogiciel { get; set; } = 0;

        [Column("COMM_RDV_COMM")]
        [StringLength(255)]
        public string? CommRdvComm { get; set; }

        [Column("CLE_AUTRE_LOGICIEL")]
        public int? CleAutreLogiciel { get; set; }

        [Column("MEM_COMM_FAIT")]
        [StringLength(255)]
        public string? MemCommFait { get; set; }

        [Column("MEM_COMM_AFAIRE")]
        [StringLength(255)]
        public string? MemCommAfaire { get; set; }

        [Column("MEM_COMM_COLSUP1")]
        [StringLength(255)]
        public string? MemCommColsup1 { get; set; }

        [Column("MEM_COMM_COLSUP2")]
        [StringLength(255)]
        public string? MemCommColsup2 { get; set; }

        [Column("MEM_COMM_COLSUP3")]
        [StringLength(255)]
        public string? MemCommColsup3 { get; set; }

        [Column("MEM_COMM_COLSUP4")]
        [StringLength(255)]
        public string? MemCommColsup4 { get; set; }

        [Column("MEM_COMM_COLSUP5")]
        [StringLength(255)]
        public string? MemCommColsup5 { get; set; }

        [Column("MEM_COMM_COLSUP6")]
        [StringLength(255)]
        public string? MemCommColsup6 { get; set; }

        [Column("MEM_COMM_COLSUP7")]
        [StringLength(255)]
        public string? MemCommColsup7 { get; set; }

        [Column("MEM_COMM_COLSUP8")]
        [StringLength(255)]
        public string? MemCommColsup8 { get; set; }

        [Column("MEM_COMM_COM1")]
        [StringLength(255)]
        public string? MemCommCom1 { get; set; }

        [Column("MEM_COMM_COM2")]
        [StringLength(255)]
        public string? MemCommCom2 { get; set; }

        [Column("MEM_COMM_COM3")]
        [StringLength(255)]
        public string? MemCommCom3 { get; set; }

        [Column("MEM_COMM_COM4")]
        [StringLength(255)]
        public string? MemCommCom4 { get; set; }

        [Column("MEM_COMM_COM5")]
        [StringLength(255)]
        public string? MemCommCom5 { get; set; }

        [Column("MEM_COMM_COM6")]
        [StringLength(255)]
        public string? MemCommCom6 { get; set; }

        [Column("MEM_COMM_COM7")]
        [StringLength(255)]
        public string? MemCommCom7 { get; set; }

        [Column("MEM_COMM_COM8")]
        [StringLength(255)]
        public string? MemCommCom8 { get; set; }

        [Column("TIMER")]
        public int Timer { get; set; } = 0;

        [Column("BGCOLOR_ELCOMMENTAIRE")]
        public int BgColorElCommentaire { get; set; } = 16777215;

        [Column("ID_STYLE")]
        public int IdStyle { get; set; } = -1;

        [Column("ID_PRAT_PROF")]
        public int IdPratProf { get; set; } = 0;

        [Column("HYGIENE_ASEPSIE")]
        [StringLength(1)]
        public string? HygieneAsepsie { get; set; }

        [Column("ORGANISATION_GESTION")]
        [StringLength(1)]
        public string? OrganisationGestion { get; set; }

        [Column("COMPORTEMENT_PROFESSIONNEL")]
        [StringLength(1)]
        public string? ComportementProfessionnel { get; set; }

        [Column("APPLICATION_CONNAISSANCES")]
        [StringLength(1)]
        public string? ApplicationConnaissances { get; set; }

        [Column("APTITUDE_GESTUELLE")]
        [StringLength(1)]
        public string? AptitudeGestuelle { get; set; }

        [Column("GESTION_POST_OPERATGOIRE")]
        [StringLength(1)]
        public string? GestionPostOperatoire { get; set; }

        [Column("CHANGE_LOCK")]
        public int ChangeLock { get; set; } = 0;

        [Column("DATE_UPDATE")]
        public DateTime? DateUpdate { get; set; }

        [Column("GUID")]
        [StringLength(16)]
        public byte[]? Guid { get; set; }

        public Patient Patient { get; set; }
    }
}
