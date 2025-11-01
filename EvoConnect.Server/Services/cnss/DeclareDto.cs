using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Services.cnss
{
    public class PrescriptionInitiationDto
    {
        public string InpeEtablissement { get; set; }
        public string InpeMedecin { get; set; }
        public PatientInitiationDto Patient { get; set; }
        public DiagnosticDto Diagnostic { get; set; }
        public bool EstAccident { get; set; }
        public string Commentaire { get; set; }
        public DateTime DateVisite { get; set; }
        public string TypeAccident { get; set; }
        public FseTypeDto FseType { get; set; }
        public List<ConstantesInitiationDto> Constantes { get; set; }
        public List<OrdonnanceInitiationDto> Ordonnances { get; set; }
        public List<ActeAdresseInitiationDto> ActesAdresses { get; set; }
        public List<ActeRealiseInitiationDto> ActeRealises { get; set; }
        public List<PrescriptionDispositifMedicalInitiationDto> DispositifMedicaux { get; set; }
    }

    public class PatientInitiationDto
    {
        public long IdPatient { get; set; }
        public string NumeroImmatriculation { get; set; }
        public string NumeroIndividu { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Genre { get; set; }
        public DateTime DateNaissance { get; set; }
    }

    public class DiagnosticDto
    {
        public List<PathologieInitiationDto> Pathologies { get; set; }
        public string Diagnostic { get; set; }
    }

    public class PathologieInitiationDto
    {
        public string CodePathologie { get; set; }
        public string Description { get; set; }
        public bool Provisoire { get; set; }
    }

    public class FseTypeDto
    {
        public string LibelleOrdonnanceType { get; set; }
        public string LibelleConsultationType { get; set; }
        public bool EstOrdonnanceType { get; set; }
        public bool EstConsultationType { get; set; }
    }

    public class ConstantesInitiationDto
    {
        public long Id { get; set; }
        public string Nom { get; set; }
        public string Valeur { get; set; }
    }

    public class OrdonnanceInitiationDto
    {
        public DateTime DateOrdonnance { get; set; }
        public List<MedicamentInitiationDto> ListMedicament { get; set; }
    }

    public class MedicamentInitiationDto
    {
        public string Code { get; set; }
        public string Libelle { get; set; }
        public string UniteParJour { get; set; }
        public string Dosage { get; set; }
        public string Forme { get; set; }
        public string UniteDosage { get; set; }
        public int NombreJour { get; set; }
        public string Commentaire { get; set; }
        public bool TraitementContinu { get; set; }
        public string Motif { get; set; }
    }

    public class ActeAdresseInitiationDto
    {
        public string Code { get; set; }
        public string Libelle { get; set; }
        public string Localisation { get; set; }
        public string CategorieActe { get; set; }
        public bool IsEP { get; set; }
        public int NombreActes { get; set; }
        public string Commentaire { get; set; }
        public string Motif { get; set; }
        public List<ActeDentaireInitiationDto> DentaireCodes { get; set; }
    }

    public class ActeRealiseInitiationDto
    {
        public string Code { get; set; }
        public string Libelle { get; set; }
        public string Localisation { get; set; }
        public string CategorieActe { get; set; }
        public bool IsEP { get; set; }
        public int NombreActes { get; set; }
        public string Commentaire { get; set; }
        public string Motif { get; set; }
        public List<ActeDentaireInitiationDto> DentaireCodes { get; set; }
        public double PrixUnitaire { get; set; }
        public DateTime DateRealisation { get; set; }
    }

    public class ActeDentaireInitiationDto
    {
        public string Code { get; set; }
        public string Type { get; set; }
        public List<FaceDto> Faces { get; set; }
    }

    public class FaceDto
    {
        public string Face { get; set; }
    }

    public class PrescriptionDispositifMedicalInitiationDto
    {
        public string Code { get; set; }
        public string Libelle { get; set; }
        public int Nombre { get; set; }
        public string Commentaire { get; set; }
        public string Motif { get; set; }
        public bool IsEP { get; set; }
    }
}
