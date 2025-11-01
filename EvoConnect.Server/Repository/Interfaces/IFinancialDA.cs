
// Interface definition
namespace EvoConnect.Server.Repository.Interfaces
{
    public interface IFinancialDA
    {
        // Invoice methods
        Task<PaginatedResult<InvoiceDto>> GetPaginatedInvoicesAsync(InvoiceFilterRequest request);
        Task<InvoiceDto> GetInvoiceByIdAsync(string invoiceId);
        Task<List<InvoiceDto>> GetInvoicesByPatientAsync(int patientId);
        Task<bool> CreateInvoiceAsync(CreateInvoiceRequest request);

        // Quote methods
        Task<PaginatedResult<QuoteDto>> GetPaginatedQuotesAsync(QuoteFilterRequest request);
        Task<QuoteDto> GetQuoteByIdAsync(int quoteId);
        Task<List<QuoteDto>> GetQuotesByPatientAsync(int patientId);
        Task<bool> UpdateQuoteAcceptanceAsync(int quoteId, bool accepted);
        Task<int> CreateQuoteAsync(CreateQuoteRequest request);
        Task<bool> UpdateQuoteAsync(int quoteId, UpdateQuoteRequest request);

        // Insurance methods
        Task<List<InsuranceDto>> GetAllInsuranceCompaniesAsync();
        Task<InsuranceDto> GetInsuranceByIdAsync(int insuranceId);

        // Statistics and reporting
        Task<FinancialStatistics> GetFinancialStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync(int year);

        // Additional utility methods
        Task<List<InvoiceDto>> GetRecentInvoicesAsync(int days = 30, int limit = 50);
        Task<List<QuoteDto>> GetPendingQuotesAsync();
        Task<float> GetPatientBalanceAsync(int patientId);
        Task<List<InvoiceDto>> GetOverdueInvoicesAsync();
        Task<List<DevisStats>> GetDevisStatsAsync(DateTime startDate, DateTime endDate);

        Task<List<MonthlyDevisStats>> GetMonthlyDevisStatsAsync(int year);

        Task<List<DailyDevisStats>> GetDailyDevisStatsAsync(DateTime fromDate, DateTime toDate);

        Task<DevisSummaryStats> GetDevisSummaryStatsAsync(DateTime fromDate, DateTime toDate);

        Task<List<DevisStateStats>> GetDevisByStateAsync(DateTime fromDate, DateTime toDate);

    }


    /// <summary>
    /// Statistics for devis with dynamic time grouping
    /// </summary>
    public class DevisStats
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public string Period { get; set; } = string.Empty;
        public int TotalDevis { get; set; }
        public int AcceptedDevis { get; set; }
        public int RefusedDevis { get; set; }
        public int PendingDevis { get; set; }
        public GroupingType GroupingType { get; set; }
    }

    /// <summary>
    /// Monthly devis statistics
    /// </summary>
    public class MonthlyDevisStats
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalDevis { get; set; }
        public int AcceptedDevis { get; set; }
        public int RefusedDevis { get; set; }
        public int PendingDevis { get; set; }
        public double AverageResponseTime { get; set; } // In days
    }

    /// <summary>
    /// Daily devis statistics
    /// </summary>
    public class DailyDevisStats
    {
        public DateTime Date { get; set; }
        public int TotalDevis { get; set; }
        public int AcceptedDevis { get; set; }
        public int RefusedDevis { get; set; }
        public int PendingDevis { get; set; }
    }

    /// <summary>
    /// Summary statistics for devis
    /// </summary>
    public class DevisSummaryStats
    {
        public int TotalDevis { get; set; }
        public int AcceptedDevis { get; set; }
        public int RefusedDevis { get; set; }
        public int PendingDevis { get; set; }
        public double AcceptanceRate { get; set; } // Percentage
        public double RefusalRate { get; set; } // Percentage
        public double AverageResponseTime { get; set; } // In days
    }

    /// <summary>
    /// Statistics by devis state
    /// </summary>
    public class DevisStateStats
    {
        public int State { get; set; }
        public int TotalDevis { get; set; }
        public int AcceptedDevis { get; set; }
        public int RefusedDevis { get; set; }
    }



    public class PatientInvoiceSummary
    {
        public int PatientId { get; set; }
        public int TotalInvoices { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPatientPortion { get; set; }
        public DateTime? LastInvoiceDate { get; set; }
        public double AverageAmount { get; set; }
    }

    public class PatientQuoteSummary
    {
        public int PatientId { get; set; }
        public int TotalQuotes { get; set; }
        public int AcceptedQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public double TotalAmount { get; set; }
        public DateTime? LastQuoteDate { get; set; }
    }

    public class InvoiceDto
    {
        public string InvoiceId { get; set; }
        public int PatientId { get; set; }
        public int UserId { get; set; }
        public float Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public float PatientPortion { get; set; }
        public string PatientLastName { get; set; }
        public string PatientFirstName { get; set; }

        public string PatientFullName => $"{PatientLastName} {PatientFirstName}".Trim();
        public float InsurancePortion => Amount - PatientPortion;
    }

    public class QuoteDto
    {
        public int QuoteId { get; set; }
        public string QuoteName { get; set; }
        public int PatientId { get; set; }
        public bool IsAccepted { get; set; }
        public float Amount { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime PrintDate { get; set; }
        public bool IsPresented { get; set; }
        public DateTime PresentationDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string PaymentMode { get; set; }
        public float Discount { get; set; }
        public float AmountBeforeDiscount { get; set; }
        public int DiscountType { get; set; }
        public string PatientLastName { get; set; }
        public string PatientFirstName { get; set; }

        public string PatientFullName => $"{PatientLastName} {PatientFirstName}".Trim();
        public float FinalAmount => AmountBeforeDiscount - Discount;
        public string Status => IsAccepted ? "Accepted" : IsPresented ? "Presented" : "Draft";
    }

    public class InsuranceDto
    {
        public int InsuranceId { get; set; }
        public int? AddressId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Number { get; set; }
    }

    public class FinancialStatistics
    {
        public int TotalInvoices { get; set; }
        public float TotalRevenue { get; set; }
        public float AverageInvoiceAmount { get; set; }
        public float TotalPatientPortion { get; set; }
        public int AcceptedQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
    }

    // Filter classes
    public class InvoiceFilters
    {
        public string? InvoiceId { get; set; }
        public int? PatientId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public float? AmountFrom { get; set; }
        public float? AmountTo { get; set; }
    }

    public class QuoteFilters
    {
        public int? PatientId { get; set; }
        public bool? IsAccepted { get; set; }
        public bool? IsPresented { get; set; }
        public DateTime? CreationDateFrom { get; set; }
        public DateTime? CreationDateTo { get; set; }
        public float? AmountFrom { get; set; }
        public float? AmountTo { get; set; }
    }

    public class InvoiceFilterRequest
    {
        public InvoiceFilters Filters { get; set; } = new InvoiceFilters();
        public PaginationRequest Pagination { get; set; } = new PaginationRequest();
        public SortingRequest Sorting { get; set; } = new SortingRequest();
    }

    public class QuoteFilterRequest
    {
        public QuoteFilters Filters { get; set; } = new QuoteFilters();
        public PaginationRequest Pagination { get; set; } = new PaginationRequest();
        public SortingRequest Sorting { get; set; } = new SortingRequest();
    }

    // Additional supporting models
    public class MonthlyRevenue
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int InvoiceCount { get; set; }
        public float TotalRevenue { get; set; }
        public float PatientPortion { get; set; }

        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM");
        public float InsurancePortion => TotalRevenue - PatientPortion;
    }

    public class CreateInvoiceRequest
    {
        public string InvoiceId { get; set; }
        public int PatientId { get; set; }
        public int UserId { get; set; }
        public float Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public float PatientPortion { get; set; }
    }

    public class CreateQuoteRequest
    {
        public string QuoteName { get; set; }
        public int PatientId { get; set; }
        public float Amount { get; set; }
        public float AmountBeforeDiscount { get; set; }
        public float Discount { get; set; }
        public int DiscountType { get; set; }
    }

    public class UpdateQuoteRequest
    {
        public string QuoteName { get; set; }
        public float Amount { get; set; }
        public float AmountBeforeDiscount { get; set; }
        public float Discount { get; set; }
        public int DiscountType { get; set; }
        public string PaymentMode { get; set; }
    }

    // Sort field constants
    public static class FinancialSortFields
    {
        // Invoice sort fields
        public const string InvoiceDate = "InvoiceDate";
        public const string InvoiceAmount = "Amount";
        public const string PatientName = "PatientName";
        public const string PatientPortion = "PatientPortion";

        // Quote sort fields
        public const string QuoteCreationDate = "CreationDate";
        public const string QuoteAmount = "Amount";
        public const string QuotePatientName = "PatientName";
        public const string QuoteStatus = "IsAccepted";
        public const string QuoteLastModified = "LastModified";
    }

}