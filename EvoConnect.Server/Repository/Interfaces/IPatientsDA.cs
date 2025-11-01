using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Repository.Interfaces
{
    public interface IPatientsDA
    {

        Task<PaginatedResult<EvocomPatientDto>> GetPaginatedPatientsAsync(PatientFilterRequest request);
        public Task<PatientEngagementStatistics> GetPatientEngagementStatisticsAsync();
        Task<DetailedPatientEngagementStatistics> GetDetailedPatientEngagementStatisticsAsync();
        public Task<int> GetLostPatients();
        public Task<int> GetTotalActivePatients();
        Task<int> GetPatientCountAsync(PatientFilters filters);
        Task<EvocomPatientDto> GetPatientByIdAsync(int patientId);
        Task<PaginatedResult<EvocomPatientDto>> SearchPatientsAsync(string searchText, int pageNumber = 1, int pageSize = 20);
        public Task<List<PatientCreationStatistic>> GetPatientCreationStatisticsAsync(DateTime fromDate, DateTime toDate);

    }
    public class DetailedPatientEngagementStatistics
{
    public int TotalPatients { get; set; }
    
    // No appointments
    public int PatientsWithNoAppointments { get; set; }
    public decimal PatientsWithNoAppointmentsPercentage { get; set; }
    
    // Appointments but never showed up
    public int PatientsWithAppointmentsButNeverShowedUp { get; set; }
    public decimal PatientsWithAppointmentsButNeverShowedUpPercentage { get; set; }
    
    // No-show appointments
    public int PatientsWithNoShowAppointments { get; set; }
    public decimal PatientsWithNoShowAppointmentsPercentage { get; set; }
    
    // Cancelled appointments
    public int PatientsWithCancelledAppointments { get; set; }
    public decimal PatientsWithCancelledAppointmentsPercentage { get; set; }
    
    // No realized treatments
    public int PatientsWithNoRealizedTreatments { get; set; }
    public decimal PatientsWithNoRealizedTreatmentsPercentage { get; set; }
    
    // Realized treatments
    public int PatientsWithRealizedTreatments { get; set; }
    public decimal PatientsWithRealizedTreatmentsPercentage { get; set; }
    
    // Planned but not realized
    public int PatientsWithPlannedButNotRealizedTreatments { get; set; }
    public decimal PatientsWithPlannedButNotRealizedTreatmentsPercentage { get; set; }
}

    public class PatientEngagementStatistics
    {
        public int TotalPatients { get; set; }
        
        // No appointments statistics
        public int PatientsWithNoAppointments { get; set; }
        public decimal PatientsWithNoAppointmentsPercentage { get; set; }
        
        // Appointments but never showed up
        public int PatientsWithAppointmentsButNeverShowedUp { get; set; }
        public decimal PatientsWithAppointmentsButNeverShowedUpPercentage { get; set; }
        
        // No realized treatments
        public int PatientsWithNoRealizedTreatments { get; set; }
        public decimal PatientsWithNoRealizedTreatmentsPercentage { get; set; }
        
        // At least one realized treatment
        public int PatientsWithRealizedTreatments { get; set; }
        public decimal PatientsWithRealizedTreatmentsPercentage { get; set; }
    }
    public class PatientFilterRequest
    {
        public PatientFilters Filters { get; set; } = new PatientFilters();
        public PaginationRequest Pagination { get; set; } = new PaginationRequest();
        public SortingRequest Sorting { get; set; } = new SortingRequest();
    }

    public class PatientFilters
    {
        // Personal Information Filters
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string SecuriteSociale { get; set; }
        public char? Gender { get; set; }
        public DateTime? BirthDateFrom { get; set; }
        public DateTime? BirthDateTo { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        // Patient-Specific Filters
        public int? DossierNumber { get; set; }
        public string DossierReference { get; set; }
        public DateTime? CreationDateFrom { get; set; }
        public DateTime? CreationDateTo { get; set; }
        public DateTime? LastAppointmentFrom { get; set; }
        public DateTime? LastAppointmentTo { get; set; }
        public DateTime? NextAppointmentFrom { get; set; }
        public DateTime? NextAppointmentTo { get; set; }

        // Financial Filters
        public decimal? BalanceFrom { get; set; }
        public decimal? BalanceTo { get; set; }
        public bool? HasPositiveBalance { get; set; }
        public bool? HasNegativeBalance { get; set; }

        // Status Filters
        public int? StatusId { get; set; }
        public bool? HasCMU { get; set; }
        public bool? HasTiersPayant { get; set; }
        public bool? HasAllergies { get; set; }
        public bool? HasALD { get; set; }

        // Treatment Filters
        public string TreatmentPhase { get; set; }
        public DateTime? TreatmentStartFrom { get; set; }
        public DateTime? TreatmentStartTo { get; set; }
        public string DossierType { get; set; }

        // Contact Filters
        public bool? EmailAuthorized { get; set; }
        public DateTime? LastModifiedFrom { get; set; }
        public DateTime? LastModifiedTo { get; set; }

        // Professional Information
        public string Profession { get; set; }
        public string Mutuelle { get; set; }

        // Advanced Filters
        public List<int> PatientTypes { get; set; } = new List<int>();
        public List<int> ExcludePatientIds { get; set; } = new List<int>();
        public bool? IsActive { get; set; }
    }

    public class PaginationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Validation
        public int ValidatedPageNumber => Math.Max(1, PageNumber);
        public int ValidatedPageSize => Math.Min(Math.Max(1, PageSize), 100); // Max 100 items per page
    }

    public class SortingRequest
    {
        public string SortBy { get; set; } = "LastName"; // Default sort
        public SortDirection Direction { get; set; } = SortDirection.Ascending;
    }

    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1
    }

    // Common sort fields enum for type safety
    public static class PatientSortFields
    {
        public const string LastName = "LastName";
        public const string FirstName = "FirstName";
        public const string DossierNumber = "DossierNumber";
        public const string CreationDate = "CreationDate";
        public const string LastAppointment = "LastAppointment";
        public const string NextAppointment = "NextAppointment";
        public const string Balance = "Balance";
        public const string LastModified = "LastModified";
        public const string BirthDate = "BirthDate";
        public const string City = "City";
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public PaginationMetadata Metadata { get; set; } = new PaginationMetadata();
    }

    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }

    public class EvocomPatientDto
    {
        // Person Information
        public int PersonId { get; set; }
        public string LastName { get; set; }
        public string MaidenName { get; set; }
        public string FirstName { get; set; }
        public char? Gender { get; set; }
        public string SecuriteSociale { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Email { get; set; }
        public string Email2 { get; set; }
        public string Email3 { get; set; }
        public string PrimaryPhone { get; set; }
        public string WorkPhone1 { get; set; }
        public string WorkPhone2 { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }

        // Address Information
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        // Professional Information
        public string Profession { get; set; }
        public string Mutuelle { get; set; }
        public string Title { get; set; }
        public string Website { get; set; }

        // Patient-Specific Information
        public int DossierNumber { get; set; }
        public string DossierReference { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? LastAppointment { get; set; }
        public DateTime? NextAppointment { get; set; }
        public decimal? Balance { get; set; }
        public decimal? BalanceEuro { get; set; }
        public string TreatmentPhase { get; set; }
        public DateTime? TreatmentStart { get; set; }
        public string DossierType { get; set; }
        public int StatusId { get; set; }
        public string StatusLabel { get; set; }

        // Medical Information
        public bool HasAllergies { get; set; }
        public bool HasCMU { get; set; }
        public bool HasTiersPayant { get; set; }
        public bool HasALD { get; set; }
        public DateTime? HospitalEntryDate { get; set; }
        public DateTime? HospitalExitDate { get; set; }

        // System Information
        public DateTime? LastModified { get; set; }
        public bool EmailAuthorized { get; set; }
        public int PatientColor { get; set; }
        public string NextPayment { get; set; }
        public DateTime? NextPaymentDate { get; set; }

        // Additional computed fields
        public int Age => BirthDate.HasValue ?
            DateTime.Now.Year - BirthDate.Value.Year -
            (DateTime.Now.DayOfYear < BirthDate.Value.DayOfYear ? 1 : 0) : 0;

        public string FullName => $"{LastName?.Trim()} {FirstName?.Trim()}".Trim();
        public bool HasUpcomingAppointment => NextAppointment.HasValue && NextAppointment > DateTime.Now;
        public int DaysSinceLastAppointment => LastAppointment.HasValue ?
            (DateTime.Now - LastAppointment.Value).Days : -1;
    }
    // Supporting models and enums
public class PatientCreationStatistic
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string PeriodLabel { get; set; }
        public PatientCreationGrouping GroupingLevel { get; set; }
    }

    public enum PatientCreationGrouping
    {
        Day,
        Month,
        Year
    }
    // Additional model for statistics
    public class PatientStatistics
    {
        public int TotalPatients { get; set; }
        public int WithUpcomingAppointments { get; set; }
        public int WithPositiveBalance { get; set; }
        public int WithNegativeBalance { get; set; }
        public int WithCMU { get; set; }
        public int WithALD { get; set; }
        public int CreatedLast30Days { get; set; }
        public decimal AverageBalance { get; set; }
        public DateTime? LastPatientCreated { get; set; }
    }
}