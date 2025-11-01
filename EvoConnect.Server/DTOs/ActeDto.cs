using System;

namespace EvoConnect.Server.DTOs
{
    public class DentalisActeDto
    {
        public int ActePk { get; set; }
        public string ActeNom { get; set; } = string.Empty;
        public string? ActeAbrege { get; set; }
        public int ActeFamille { get; set; }
        public string? FamilleNom { get; set; }
        public int ActeCodeSecu { get; set; }
        public CodeSecuDto? CodeSecu { get; set; }
        public short ActeNbDents { get; set; }
        public float? ActeMontantA { get; set; }
        public float? ActeMontantE { get; set; }
        public float? ActeMontantU { get; set; }
        public DateTime ActeDateCreation { get; set; }
        public DateTime ActeDateMaj { get; set; }
    }

    public class CodeSecuDto
    {
        public int IdCodeSecu { get; set; }
        public string CodeNom { get; set; } = string.Empty;
        public string CodeLibelle { get; set; } = string.Empty;
        public double? CodeValeur { get; set; }
        public double? CodeValeurEuro { get; set; }
        public float TauxRembours { get; set; }
        public string? CodePrestation { get; set; }
        public double CodeCoeff { get; set; }
    }

    public class ActePatientDto
    {
        public int ApPk { get; set; }
        public int ApPatient { get; set; }
        public int ApActe { get; set; }
        public string? ActeNom { get; set; }
        public string? ApLibelleActe { get; set; }
        public short ApRealise { get; set; }
        public DateTime ApDateRealise { get; set; }
        public short ApDentFrom { get; set; }
        public short ApDentTo { get; set; }
        public string? ApSelectedDents { get; set; }
        public float ApMontant { get; set; }
        public CodeSecuDto? CodeSecu { get; set; }
        public DateTime ApDateCreation { get; set; }
        public short ApFinalise { get; set; }
        public DateTime? ApDateFinalise { get; set; }
        public string ApFacture { get; set; } = string.Empty;
        public DateTime? ApFactureDate { get; set; }
    }
}