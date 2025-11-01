using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvoConnect.Server.Repository.Interfaces
{
    public interface IPaymentDA
    {
        Task<PaginatedResult<PaymentDto>> GetPaginatedPaymentsAsync(PaymentFilterRequest request);

        Task<int> GetPaymentCountAsync(PaymentFilters filters);

        Task<List<PaymentDto>> GetOverduePaymentsAsync(int daysPastDue = 30);

        Task<List<PaymentDto>> GetPaymentsByBankDepositAsync(DateTime depositDate);

        Task<PaymentSummary> GetPaymentsSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);

        Task<List<PaymentDto>> GetPaymentsByPatientIdAsync(int patientId);
        public Task<float> GetTotalPaymentsToday();

        /// <summary>
        /// Get payment details by ID
        /// </summary>
        Task<PaymentDto> GetPaymentByIdAsync(int patientId, int codePlan, int numDate);

        /// <summary>
        /// Get payments by date range
        /// </summary>
        Task<List<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get outstanding payments
        /// </summary>
        Task<List<PaymentDto>> GetOutstandingPaymentsAsync();

        /// <summary>
        /// Get payment statistics for patient
        /// </summary>
        Task<PaymentStatistics> GetPaymentStatisticsAsync(int patientId);

        /// <summary>
        /// Get payments by payment mode
        /// </summary>
        Task<List<PaymentDto>> GetPaymentsByModeAsync(PaymentMode paymentMode);

        /// <summary>
        /// Update payment status
        /// </summary>
        Task<bool> UpdatePaymentStatusAsync(int patientId, int codePlan, int numDate, PaymentStatus status);

        // NEW METHODS - Add to interface
        /// <summary>
        /// Get comprehensive payment statistics for dashboard in a single query
        /// </summary>
        Task<PaymentDashboardStats> GetPaymentDashboardStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Get monthly revenue stats for a specific year (chiffre d'affaire par mois)
        /// </summary>
        Task<Dictionary<int, float>> GetMonthlyRevenueAsync(int year);

        /// <summary>
        /// Get weekly revenue stats for current year (chiffre d'affaire par semaine)
        /// </summary>
        Task<Dictionary<int, float>> GetWeeklyRevenueAsync(int year);

        /// <summary>
        /// Get yearly revenue comparison for multiple years (chiffre d'affaire par année)
        /// </summary>
        Task<Dictionary<int, float>> GetYearlyRevenueAsync(int startYear, int endYear);

        /// <summary>
        /// Get top patients by payment amount for optimized dashboard
        /// </summary>
        Task<List<TopPatientPayment>> GetTopPatientsByPaymentAsync(int limit = 10, DateTime? fromDate = null, DateTime? toDate = null);
    }

    public class PaymentFilterRequest
    {
        public PaymentFilters Filters { get; set; } = new PaymentFilters();
        public PaginationRequest Pagination { get; set; } = new PaginationRequest();
        public PaymentSortingRequest Sorting { get; set; } = new PaymentSortingRequest();
    }

    public class PaymentFilters
    {
        // Patient filters
        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
        public int? DossierNumber { get; set; }

        // Payment filters
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? PaymentRefDateFrom { get; set; }
        public DateTime? PaymentRefDateTo { get; set; }

        // Amount filters
        public float? AmountFrom { get; set; }
        public float? AmountTo { get; set; }
        public float? AmountEuroFrom { get; set; }
        public float? AmountEuroTo { get; set; }
        public float? CollectedAmountFrom { get; set; }
        public float? CollectedAmountTo { get; set; }

        // Type and mode filters
        public int? PaymentType { get; set; }
        public PaymentMode? PaymentMode { get; set; }
        public string? PaymentLabel { get; set; }

        // Bank filters
        public string? BankFlag { get; set; }
        public string? BankName { get; set; }
        public string? CheckNumber { get; set; }

        // Medical filters
        public bool? HasALD { get; set; }
        public bool? HasExoneration { get; set; }
        public bool? IsAccident { get; set; }

        // Plan filters
        public int? PlanCode { get; set; }
        public int? PlanId { get; set; }

        // Practitioner filter
        public int? PractitionerId { get; set; }

        // Status filters
        public bool? IsPaid { get; set; }
        public bool? HasOutstanding { get; set; }

        // Insurance filters
        public int? MutuelleId { get; set; }
        public int? CaisseId { get; set; }

        // Date filters for specific events
        public DateTime? CreationDateFrom { get; set; }
        public DateTime? CreationDateTo { get; set; }
        public DateTime? BankDepositDateFrom { get; set; }
        public DateTime? BankDepositDateTo { get; set; }
    }

    public class PaymentSortingRequest
    {
        public string SortBy { get; set; } = PaymentSortFields.Date;
        public SortDirection Direction { get; set; } = SortDirection.Descending;
    }

    public static class PaymentSortFields
    {
        public const string Date = "Date";
        public const string PaymentRefDate = "PaymentRefDate";
        public const string Amount = "Amount";
        public const string CollectedAmount = "CollectedAmount";
        public const string PatientName = "PatientName";
        public const string PaymentType = "PaymentType";
        public const string PaymentMode = "PaymentMode";
        public const string PlanCode = "PlanCode";
        public const string BankDeposit = "BankDeposit";
    }

    public enum PaymentMode
    {
        Cash = 0,
        CCP = 1,
        BankCheck = 2,
        CB = 3,
        Other = 4,
        BankTransfer = 5,
        Withdrawal = 6,
        Supported = 7,
        Promotion = 8,
        BankDeposit = 9,
        CKMFondation = 10,
        Cangotte = 11,
        MCICare = 12,
        MCICARE_FKCM = 13,
        RTS = 14
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        PartiallyPaid = 2,
        Overdue = 3,
        Cancelled = 4
    }

    public class PaymentDto
    {
        // Primary keys
        public int PatientId { get; set; }
        public int PlanCode { get; set; }
        public int NumDate { get; set; }

        // Patient information
        public string PatientName { get; set; }
        public string PatientFirstName { get; set; }
        public int DossierNumber { get; set; }

        // Payment dates
        public DateTime? Date { get; set; }
        public DateTime? PaymentRefDate { get; set; }
        public DateTime? ProposedDate { get; set; }
        public DateTime? ExpectedDepositDate { get; set; }

        // Payment details
        public int PaymentType { get; set; }
        public string PaymentLabel { get; set; }
        public float? Amount { get; set; }
        public float? AmountEuro { get; set; }
        public float? CollectedAmount { get; set; }
        public float? CollectedAmountEuro { get; set; }

        // Payment method
        public PaymentMode? PaymentModeValue { get; set; }
        public string PaymentModeLabel { get; set; }

        // Bank information
        public string BankFlag { get; set; }
        public string BankName { get; set; }
        public string CheckNumber { get; set; }
        public DateTime? BankDepositDate { get; set; }

        // Plan information
        public int? PlanId { get; set; }
        public string Tariff { get; set; }
        public int? DateRef { get; set; }
        public int? NbDays { get; set; }
        public int? NbMonths { get; set; }

        // Medical information
        public bool HasALD { get; set; }
        public bool HasExoneration { get; set; }
        public bool IsAccident { get; set; }
        public bool IsSundayHoliday { get; set; }
        public bool IsNight { get; set; }
        public DateTime? AccidentDate { get; set; }
        public int? ToothNumber { get; set; }
        public string TeethList { get; set; }

        // Insurance information
        public float? ThirdPartyAmount { get; set; }
        public float? MutuelleAmount { get; set; }
        public int? MutuelleId { get; set; }
        public int? CaisseId { get; set; }
        public string MutuelleFlag { get; set; }

        // FSE (Feuille de Soins Électronique) information
        public int? FSENumber { get; set; }
        public int? FSELineNumber { get; set; }
        public DateTime? FSEDate { get; set; }
        public string FSEMode { get; set; }
        public bool IsFSEFormatted { get; set; }

        // Practitioner information
        public int? PractitionerId { get; set; }
        public string PractitionerName { get; set; }

        // Additional information
        public string Semester { get; set; }
        public string Remarks { get; set; }
        public int? MultiPaymentId { get; set; }
        public bool IsMultiPayment { get; set; }

        // System information
        public string CreatedWithCurrency { get; set; }
        public DateTime? PrintDate { get; set; }
        public int? LogUser { get; set; }
        public string RUMNumber { get; set; }

        // Computed properties
        public float OutstandingAmount => (Amount ?? 0) - (CollectedAmount ?? 0);
        public bool IsFullyPaid => Math.Abs(OutstandingAmount) < 0.01;
        public bool HasOutstanding => OutstandingAmount > 0.01;
        public PaymentStatus Status => GetPaymentStatus();

        private PaymentStatus GetPaymentStatus()
        {
            if (IsFullyPaid) return PaymentStatus.Paid;
            if (CollectedAmount > 0) return PaymentStatus.PartiallyPaid;
            if (PaymentRefDate.HasValue && PaymentRefDate < DateTime.Now.AddDays(-30))
                return PaymentStatus.Overdue;
            return PaymentStatus.Pending;
        }

        public string GetPaymentModeLabel()
        {
            return PaymentModeValue switch
            {
                PaymentMode.Cash => "Espèces",
                PaymentMode.CCP => "Compte chèque postal",
                PaymentMode.BankCheck => "Chèque bancaire",
                PaymentMode.CB => "Carte bancaire",
                PaymentMode.Other => "Autre",
                PaymentMode.BankTransfer => "Virement bancaire",
                PaymentMode.Withdrawal => "Prélèvement",
                PaymentMode.Supported => "Pris en charge",
                PaymentMode.Promotion => "Promotion",
                PaymentMode.BankDeposit => "Dépôt bancaire",
                PaymentMode.CKMFondation => "Fondation CKM",
                PaymentMode.Cangotte => "Cagnotte",
                PaymentMode.MCICare => "MCI Care",
                PaymentMode.MCICARE_FKCM => "MCI Care FKCM",
                PaymentMode.RTS => "RTS",
                _ => "Inconnu"
            };
        }

        public string FullPatientName => $"{PatientName} {PatientFirstName}".Trim();
    }

    public class PaymentStatistics
    {
        public int PatientId { get; set; }
        public int TotalPayments { get; set; }
        public float TotalAmount { get; set; }
        public float TotalAmountEuro { get; set; }
        public float TotalCollected { get; set; }
        public float TotalCollectedEuro { get; set; }
        public float TotalOutstanding { get; set; }
        public float TotalOutstandingEuro { get; set; }
        public int PaidPayments { get; set; }
        public int PendingPayments { get; set; }
        public int OverduePayments { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextPaymentDate { get; set; }
        public float AveragePaymentAmount { get; set; }
        public Dictionary<PaymentMode, int> PaymentModeDistribution { get; set; } = new();
    }

    // Extension method for PaymentFilters
    public static class PaymentFiltersExtensions
    {
        public static PaymentFilters CopyWith(this PaymentFilters filters,
            int? patientId = null,
            string patientName = null,
            int? dossierNumber = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            DateTime? paymentRefDateFrom = null,
            DateTime? paymentRefDateTo = null,
            float? amountFrom = null,
            float? amountTo = null,
            float? amountEuroFrom = null,
            float? amountEuroTo = null,
            float? collectedAmountFrom = null,
            float? collectedAmountTo = null,
            int? paymentType = null,
            PaymentMode? paymentMode = null,
            string paymentLabel = null,
            string bankFlag = null,
            string bankName = null,
            string checkNumber = null,
            bool? hasALD = null,
            bool? hasExoneration = null,
            bool? isAccident = null,
            int? planCode = null,
            int? planId = null,
            int? practitionerId = null,
            bool? isPaid = null,
            bool? hasOutstanding = null,
            int? mutuelleId = null,
            int? caisseId = null,
            DateTime? creationDateFrom = null,
            DateTime? creationDateTo = null,
            DateTime? bankDepositDateFrom = null,
            DateTime? bankDepositDateTo = null)
        {
            return new PaymentFilters
            {
                PatientId = patientId ?? filters.PatientId,
                PatientName = patientName ?? filters.PatientName,
                DossierNumber = dossierNumber ?? filters.DossierNumber,
                DateFrom = dateFrom ?? filters.DateFrom,
                DateTo = dateTo ?? filters.DateTo,
                PaymentRefDateFrom = paymentRefDateFrom ?? filters.PaymentRefDateFrom,
                PaymentRefDateTo = paymentRefDateTo ?? filters.PaymentRefDateTo,
                AmountFrom = amountFrom ?? filters.AmountFrom,
                AmountTo = amountTo ?? filters.AmountTo,
                AmountEuroFrom = amountEuroFrom ?? filters.AmountEuroFrom,
                AmountEuroTo = amountEuroTo ?? filters.AmountEuroTo,
                CollectedAmountFrom = collectedAmountFrom ?? filters.CollectedAmountFrom,
                CollectedAmountTo = collectedAmountTo ?? filters.CollectedAmountTo,
                PaymentType = paymentType ?? filters.PaymentType,
                PaymentMode = paymentMode ?? filters.PaymentMode,
                PaymentLabel = paymentLabel ?? filters.PaymentLabel,
                BankFlag = bankFlag ?? filters.BankFlag,
                BankName = bankName ?? filters.BankName,
                CheckNumber = checkNumber ?? filters.CheckNumber,
                HasALD = hasALD ?? filters.HasALD,
                HasExoneration = hasExoneration ?? filters.HasExoneration,
                IsAccident = isAccident ?? filters.IsAccident,
                PlanCode = planCode ?? filters.PlanCode,
                PlanId = planId ?? filters.PlanId,
                PractitionerId = practitionerId ?? filters.PractitionerId,
                IsPaid = isPaid ?? filters.IsPaid,
                HasOutstanding = hasOutstanding ?? filters.HasOutstanding,
                MutuelleId = mutuelleId ?? filters.MutuelleId,
                CaisseId = caisseId ?? filters.CaisseId,
                CreationDateFrom = creationDateFrom ?? filters.CreationDateFrom,
                CreationDateTo = creationDateTo ?? filters.CreationDateTo,
                BankDepositDateFrom = bankDepositDateFrom ?? filters.BankDepositDateFrom,
                BankDepositDateTo = bankDepositDateTo ?? filters.BankDepositDateTo
            };
        }
    }

    // UPDATED - Fixed to handle dynamic payment modes
    public class PaymentDashboardStats
    {
        public int TotalPayments { get; set; }
        public float TotalAmount { get; set; }
        public float TotalAmountEuro { get; set; }
        public float AveragePaymentAmount { get; set; }

        // Use int keys to handle any payment modes dynamically
        public Dictionary<int, int> PaymentModeCounts { get; set; } = new();
        public Dictionary<int, float> PaymentModeAmounts { get; set; } = new();

        public float MaxPaymentAmount { get; set; }
        public float MinPaymentAmount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? FirstPaymentDate { get; set; }

        // Helper methods to get payment mode info
        public int GetModeCount(int modeId) => PaymentModeCounts.GetValueOrDefault(modeId, 0);
        public float GetModeAmount(int modeId) => PaymentModeAmounts.GetValueOrDefault(modeId, 0.0f);

        // Get all unique payment modes found in the data
        public List<int> GetAllPaymentModes()
        {
            var modes = new HashSet<int>();
            modes.UnionWith(PaymentModeCounts.Keys);
            modes.UnionWith(PaymentModeAmounts.Keys);
            return modes.OrderBy(x => x).ToList();
        }

        // Helper method to get payment mode label by ID
        public string GetPaymentModeLabel(int modeId)
        {
            if (Enum.IsDefined(typeof(PaymentMode), modeId))
            {
                var mode = (PaymentMode)modeId;
                return mode switch
                {
                    PaymentMode.Cash => "Espèces",
                    PaymentMode.CCP => "Compte chèque postal",
                    PaymentMode.BankCheck => "Chèque bancaire",
                    PaymentMode.CB => "Carte bancaire",
                    PaymentMode.Other => "Autre",
                    PaymentMode.BankTransfer => "Virement bancaire",
                    PaymentMode.Withdrawal => "Prélèvement",
                    PaymentMode.Supported => "Pris en charge",
                    PaymentMode.Promotion => "Promotion",
                    PaymentMode.BankDeposit => "Dépôt bancaire",
                    PaymentMode.CKMFondation => "Fondation CKM",
                    PaymentMode.Cangotte => "Cagnotte",
                    PaymentMode.MCICare => "MCI Care",
                    PaymentMode.MCICARE_FKCM => "MCI Care FKCM",
                    PaymentMode.RTS => "RTS",
                    _ => $"Mode {modeId}"
                };
            }
            return $"Mode {modeId}";
        }

        // Convert to old format if needed for backward compatibility
        public Dictionary<PaymentMode, int> GetPaymentModeCountsAsEnum()
        {
            var result = new Dictionary<PaymentMode, int>();
            foreach (var kvp in PaymentModeCounts)
            {
                if (Enum.IsDefined(typeof(PaymentMode), kvp.Key))
                {
                    result[(PaymentMode)kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        public Dictionary<PaymentMode, float> GetPaymentModeAmountsAsEnum()
        {
            var result = new Dictionary<PaymentMode, float>();
            foreach (var kvp in PaymentModeAmounts)
            {
                if (Enum.IsDefined(typeof(PaymentMode), kvp.Key))
                {
                    result[(PaymentMode)kvp.Key] = kvp.Value;
                }
            }
            return result;
        }
    }

    public class TopPatientPayment
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = "";
        public string PatientFirstName { get; set; } = "";
        public int DossierNumber { get; set; }
        public int PaymentCount { get; set; }
        public float TotalAmount { get; set; }
        public float AverageAmount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string FullName => $"{PatientName} {PatientFirstName}".Trim();
    }
        public class PaymentSummary
    {
        public int TotalPayments { get; set; }
        public float TotalAmount { get; set; }
        public float TotalAmountEuro { get; set; }
        public float TotalCollected { get; set; }
        public float TotalCollectedEuro { get; set; }
        public float TotalOutstanding { get; set; }
        public int OutstandingCount { get; set; }
        public int PaidCount { get; set; }
        public float AveragePaymentAmount { get; set; }

        public float CollectionRate => TotalAmount > 0 ? (TotalCollected / TotalAmount) * 100 : 0;
        public float OutstandingPercentage => TotalAmount > 0 ? (TotalOutstanding / TotalAmount) * 100 : 0;
    }
}