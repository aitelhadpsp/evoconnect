using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Repository.Interfaces
{
    public interface IAppointmentsDA
    {
        Task<List<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<List<MonthlyAppointmentStats>> GetMonthlyAppointmentStatsAsync(int year);
        Task<List<ActeStats>> GetAppointmentsByActeAsync(DateTime fromDate, DateTime toDate);
        Task<List<FauteuilStats>> GetAppointmentsByFauteuilAsync(DateTime fromDate, DateTime toDate);
        Task<AppointmentSummaryStats> GetAppointmentSummaryStatsAsync(DateTime fromDate, DateTime toDate);
        Task<List<DailyAppointmentStats>> GetDailyAppointmentStatsAsync(DateTime fromDate, DateTime toDate);
        Task<List<AppointmentDto>> GetUpcomingAppointmentsAsync(int days = 7);
        Task<List<ActeDto>> GetAllActesAsync();
        Task<List<FauteuilDto>> GetAllFauteuilsAsync();
        Task<List<AppointmentStats>> GetAppointmentStatsAsync(DateTime startDate, DateTime endDate);
        
    }

public enum GroupingType
{
    Hour,
    Day,
    Month,
    Year
}

public class AppointmentStats
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public string Period { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public double AverageDuration { get; set; }
    public GroupingType GroupingType { get; set; }
}

    public class AppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int PraticienId { get; set; }
        public int ActeId { get; set; }
        public int FauteuilId { get; set; }
        public DateTime DateRendezVous { get; set; }
        public int Duree { get; set; }
        public int? Statut { get; set; }
        public DateTime? HeureArrivee { get; set; }
        public string Commentaire { get; set; } = "";
        public string CommentaireInternet { get; set; } = "";
        public DateTime? LastModified { get; set; }
        public DateTime? HeureFauteuil { get; set; }
        public DateTime? HeureSalleAttente { get; set; }
        public DateTime? HeureSecretariat { get; set; }
        public DateTime? HeureSortie { get; set; }
        public string StatutConfirmation { get; set; } = "";
        
        // Joined data
        public string PatientNom { get; set; } = "";
        public string PatientPrenom { get; set; } = "";
        public string PatientTelephone { get; set; } = "";
        public string ActeLibelle { get; set; } = "";
        public int ActeCouleur { get; set; }
        public int? TypeActe { get; set; }
        public string FauteuilLibelle { get; set; } = "";
        
        // Computed properties
        public string PatientFullName => $"{PatientNom} {PatientPrenom}".Trim();
        public bool IsCompleted => Statut == 1;
        public bool IsCancelled => Statut == 0;
        public bool HasArrived => HeureArrivee.HasValue;
        public TimeSpan? WaitingTime => HeureArrivee.HasValue && HeureFauteuil.HasValue ? 
            HeureFauteuil.Value - HeureArrivee.Value : null;
    }

    public class MonthlyAppointmentStats
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public double AverageDuration { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM");
    }

    public class ActeStats
    {
        public int ActeId { get; set; }
        public string ActeLibelle { get; set; } = "";
        public int ActeCouleur { get; set; }
        public int TypeActe { get; set; }
        public int TotalAppointments { get; set; }
        public double AverageDuration { get; set; }
        public int TotalDuration { get; set; }
        public double Percentage { get; set; }
    }

    public class FauteuilStats
    {
        public int FauteuilId { get; set; }
        public string FauteuilLibelle { get; set; } = "";
        public int? PraticienId { get; set; }
        public int TotalAppointments { get; set; }
        public double AverageDuration { get; set; }
        public float TotalDuration { get; set; }
        public double Percentage { get; set; }
    }

    public class AppointmentSummaryStats
    {
        public int TotalAppointments { get; set; }
        public int UniquePatients { get; set; }
        public int UniqueActes { get; set; }
        public int UniqueFauteuils { get; set; }
        public double AverageDuration { get; set; }
        public int TotalDuration { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int ArrivedAppointments { get; set; }
        
        public double CompletionRate => TotalAppointments > 0 ? 
            (double)CompletedAppointments / TotalAppointments * 100 : 0;
        public double CancellationRate => TotalAppointments > 0 ? 
            (double)CancelledAppointments / TotalAppointments * 100 : 0;
        public double ArrivalRate => TotalAppointments > 0 ? 
            (double)ArrivedAppointments / TotalAppointments * 100 : 0;
    }

    public class DailyAppointmentStats
    {
        public DateTime Date { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int ArrivedAppointments { get; set; }
        public double AverageDuration { get; set; }
    }

    public class ActeDto
    {
        public int Id { get; set; }
        public string Libelle { get; set; } = "";
        public int DureeStandard { get; set; }
        public int Couleur { get; set; }
        public int? TypeActe { get; set; }
        public double? NbFauteuilBloc { get; set; }
        public string CodePlanning { get; set; } = "";
        public int? TempsChrono { get; set; }
        public int AnticipationRdv { get; set; }
        public bool NoPrint { get; set; }
        public bool Sms { get; set; }
        public int Courrier { get; set; }
        public int AnticipationSms { get; set; }
        public int CategorieStats { get; set; }
        public float Tarif { get; set; }
    }

    public class FauteuilDto
    {
        public int Id { get; set; }
        public string Libelle { get; set; } = "";
        public int? PraticienId { get; set; }
    }

}