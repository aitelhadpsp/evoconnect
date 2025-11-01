using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EvoConnect.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentDA _paymentDA;

        public PaymentController(IPaymentDA paymentDA)
        {
            _paymentDA = paymentDA;
        }

        /// <summary>
        /// Get paginated payments with filters
        /// </summary>
        [HttpPost("paginated")]
        public async Task<IActionResult> GetPaginatedPayments([FromBody] PaymentFilterRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            try
            {
                var result = await _paymentDA.GetPaginatedPaymentsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get paginated payments with query parameters (simplified version)
        /// </summary>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginatedPaymentsQuery(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? patientId = null,
            [FromQuery] string? patientName = null,
            [FromQuery] int? dossierNumber = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] float? amountFrom = null,
            [FromQuery] float? amountTo = null,
            [FromQuery] int? paymentType = null,
            [FromQuery] PaymentMode? paymentMode = null,
            [FromQuery] bool? isPaid = null,
            [FromQuery] bool? hasOutstanding = null,
            [FromQuery] string sortBy = PaymentSortFields.Date,
            [FromQuery] string sortDirection = "Descending")
        {
            try
            {
                var request = new PaymentFilterRequest
                {
                    Filters = new PaymentFilters
                    {
                        PatientId = patientId,
                        PatientName = patientName,
                        DossierNumber = dossierNumber,
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        AmountFrom = amountFrom,
                        AmountTo = amountTo,
                        PaymentType = paymentType,
                        PaymentMode = paymentMode,
                        IsPaid = isPaid,
                        HasOutstanding = hasOutstanding
                    },
                    Pagination = new PaginationRequest
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    },
                    Sorting = new PaymentSortingRequest
                    {
                        SortBy = sortBy,
                        Direction = Enum.TryParse<SortDirection>(sortDirection, out var direction)
                            ? direction
                            : SortDirection.Descending
                    }
                };

                var result = await _paymentDA.GetPaginatedPaymentsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get payment count with filters
        /// </summary>
        [HttpPost("count")]
        public async Task<IActionResult> GetPaymentCount([FromBody] PaymentFilters filters)
        {
            if (filters == null)
            {
                return BadRequest("Filters are required");
            }

            try
            {
                var count = await _paymentDA.GetPaymentCountAsync(filters);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all payments for a specific patient
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPaymentsByPatient(int patientId)
        {
            if (patientId <= 0)
            {
                return BadRequest("Valid patient ID is required");
            }

            try
            {
                var payments = await _paymentDA.GetPaymentsByPatientIdAsync(patientId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get specific payment details
        /// </summary>
        [HttpGet("{patientId}/{codePlan}/{numDate}")]
        public async Task<IActionResult> GetPaymentById(int patientId, int codePlan, int numDate)
        {
            if (patientId <= 0 || codePlan <= 0 || numDate <= 0)
            {
                return BadRequest("Valid payment identifiers are required");
            }

            try
            {
                var payment = await _paymentDA.GetPaymentByIdAsync(patientId, codePlan, numDate);

                if (payment == null)
                {
                    return NotFound($"Payment not found for patient {patientId}, plan {codePlan}, date {numDate}");
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get payments within a date range
        /// </summary>
        [HttpGet("date-range")]
        public async Task<IActionResult> GetPaymentsByDateRange(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            if (toDate.Subtract(fromDate).Days > 365)
            {
                return BadRequest("Date range cannot exceed 365 days");
            }

            try
            {
                var payments = await _paymentDA.GetPaymentsByDateRangeAsync(fromDate, toDate);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get outstanding payments
        /// </summary>
        [HttpGet("outstanding")]
        public async Task<IActionResult> GetOutstandingPayments()
        {
            try
            {
                var payments = await _paymentDA.GetOutstandingPaymentsAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get overdue payments
        /// </summary>
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverduePayments([FromQuery] int daysPastDue = 30)
        {
            if (daysPastDue <= 0 || daysPastDue > 365)
            {
                return BadRequest("Days past due must be between 1 and 365");
            }

            try
            {
                var payments = await _paymentDA.GetOverduePaymentsAsync(daysPastDue);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get payment statistics for a patient
        /// </summary>
        [HttpGet("patient/{patientId}/statistics")]
        public async Task<IActionResult> GetPaymentStatistics(int patientId)
        {
            if (patientId <= 0)
            {
                return BadRequest("Valid patient ID is required");
            }

            try
            {
                var statistics = await _paymentDA.GetPaymentStatisticsAsync(patientId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get payments by payment mode
        /// </summary>
        [HttpGet("by-mode/{paymentMode}")]
        public async Task<IActionResult> GetPaymentsByMode(PaymentMode paymentMode)
        {
            try
            {
                var payments = await _paymentDA.GetPaymentsByModeAsync(paymentMode);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get payments by bank deposit date
        /// </summary>
        [HttpGet("bank-deposit/{depositDate}")]
        public async Task<IActionResult> GetPaymentsByBankDeposit(DateTime depositDate)
        {
            try
            {
                var payments = await _paymentDA.GetPaymentsByBankDepositAsync(depositDate);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get payments summary for dashboard
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetPaymentsSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var summary = await _paymentDA.GetPaymentsSummaryAsync(fromDate, toDate);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update payment status
        /// </summary>
        [HttpPut("{patientId}/{codePlan}/{numDate}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(
            int patientId,
            int codePlan,
            int numDate,
            [FromBody] UpdatePaymentStatusRequest request)
        {
            if (patientId <= 0 || codePlan <= 0 || numDate <= 0)
            {
                return BadRequest("Valid payment identifiers are required");
            }

            if (request == null || !Enum.IsDefined(typeof(PaymentStatus), request.Status))
            {
                return BadRequest("Valid payment status is required");
            }

            try
            {
                var success = await _paymentDA.UpdatePaymentStatusAsync(patientId, codePlan, numDate, request.Status);

                if (success)
                {
                    return Ok(new { message = "Payment status updated successfully" });
                }

                return NotFound($"Payment not found for patient {patientId}, plan {codePlan}, date {numDate}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #region NEW OPTIMIZED DASHBOARD ENDPOINTS

        /// <summary>
        /// Get comprehensive dashboard statistics (optimized single call)
        /// </summary>
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetPaymentDashboardStats(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var stats = await _paymentDA.GetPaymentDashboardStatsAsync(fromDate, toDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get monthly revenue for a specific year (chiffre d'affaire par mois)
        /// </summary>
        [HttpGet("revenue/monthly/{year}")]
        public async Task<IActionResult> GetMonthlyRevenue(int year)
        {
            if (year < 2000 || year > DateTime.Now.Year + 1)
            {
                return BadRequest("Invalid year provided");
            }

            try
            {
                var monthlyRevenue = await _paymentDA.GetMonthlyRevenueAsync(year);

                // Transform to more user-friendly format
                var result = monthlyRevenue.Select(kvp => new
                {
                    Month = kvp.Key,
                    MonthName = GetMonthName(kvp.Key),
                    Revenue = kvp.Value
                }).OrderBy(x => x.Month).ToList();

                return Ok(new { Year = year, MonthlyRevenue = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get weekly revenue for a specific year (chiffre d'affaire par semaine)
        /// </summary>
        [HttpGet("revenue/weekly/{year}")]
        public async Task<IActionResult> GetWeeklyRevenue(int year)
        {
            if (year < 2000 || year > DateTime.Now.Year + 1)
            {
                return BadRequest("Invalid year provided");
            }

            try
            {
                var weeklyRevenue = await _paymentDA.GetWeeklyRevenueAsync(year);

                var result = weeklyRevenue.Select(kvp => new
                {
                    Week = kvp.Key,
                    Revenue = kvp.Value
                }).OrderBy(x => x.Week).ToList();

                return Ok(new { Year = year, WeeklyRevenue = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get yearly revenue comparison (chiffre d'affaire par année)
        /// </summary>
        [HttpGet("revenue/yearly")]
        public async Task<IActionResult> GetYearlyRevenue(
            [FromQuery] int? startYear = null,
            [FromQuery] int? endYear = null)
        {
            var currentYear = DateTime.Now.Year;
            var start = startYear ?? currentYear - 4; // Default to last 5 years
            var end = endYear ?? currentYear;

            if (start < 2000 || end > currentYear + 1 || start > end)
            {
                return BadRequest("Invalid year range provided");
            }

            if (end - start > 10)
            {
                return BadRequest("Year range cannot exceed 10 years");
            }

            try
            {
                var yearlyRevenue = await _paymentDA.GetYearlyRevenueAsync(start, end);

                var result = yearlyRevenue.Select(kvp => new
                {
                    Year = kvp.Key,
                    Revenue = kvp.Value
                }).OrderBy(x => x.Year).ToList();

                return Ok(new { YearRange = new { Start = start, End = end }, YearlyRevenue = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get top patients by payment amount
        /// </summary>
        [HttpGet("dashboard/top-patients")]
        public async Task<IActionResult> GetTopPatientsByPayment(
            [FromQuery] int limit = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (limit <= 0 || limit > 50)
            {
                return BadRequest("Limit must be between 1 and 50");
            }

            try
            {
                var topPatients = await _paymentDA.GetTopPatientsByPaymentAsync(limit, fromDate, toDate);
                return Ok(topPatients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get optimized dashboard data (single endpoint for all dashboard needs)
        /// </summary>
        [HttpGet("dashboard/optimized")]
        public async Task<IActionResult> GetOptimizedDashboard(
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
    [FromQuery] int currentYear = 0)
        {
            try
            {
                // Default to current month if no dates provided
                var from = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var to = toDate ?? DateTime.Now;
                var year = currentYear == 0 ? DateTime.Now.Year : currentYear;

                // Kick off tasks in parallel
                var stats =await _paymentDA.GetPaymentDashboardStatsAsync(from, to);
                var revenue =await _paymentDA.GetMonthlyRevenueAsync(year);
                var topPatients =await _paymentDA.GetTopPatientsByPaymentAsync(5, from, to);
                var overdue =await _paymentDA.GetOverduePaymentsAsync(30);

               

                var dashboard = new
                {
                    Period = new { From = from, To = to, Year = year },
                    Stats = stats,
                    MonthlyRevenue = revenue.Select(kvp => new
                    {
                        Month = kvp.Key,
                        MonthName = GetMonthName(kvp.Key),
                        Revenue = kvp.Value
                    }).OrderBy(x => x.Month).ToArray(),
                    TopPatients = topPatients,
                    OverduePayments = overdue.Take(10).ToArray(),
                    OverdueCount = overdue.Count
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        /// <summary>
        /// Get payment modes enum values for dropdowns
        /// </summary>
        [HttpGet("payment-modes")]
        public IActionResult GetPaymentModes()
        {
            try
            {
                var paymentModes = Enum.GetValues<PaymentMode>()
                    .Select(mode => new
                    {
                        Value = (int)mode,
                        Name = mode.ToString(),
                        Label = GetPaymentModeLabel(mode)
                    })
                    .ToList();

                return Ok(paymentModes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get payment status enum values for dropdowns
        /// </summary>
        [HttpGet("payment-statuses")]
        public IActionResult GetPaymentStatuses()
        {
            try
            {
                var paymentStatuses = Enum.GetValues<PaymentStatus>()
                    .Select(status => new
                    {
                        Value = (int)status,
                        Name = status.ToString(),
                        Label = GetPaymentStatusLabel(status)
                    })
                    .ToList();

                return Ok(paymentStatuses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get dashboard data for payments overview (legacy - kept for backward compatibility)
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetPaymentsDashboard(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // Default to current month if no dates provided
                var from = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var to = toDate ?? DateTime.Now;

                var summary = await _paymentDA.GetPaymentsSummaryAsync(from, to);
                var outstanding = await _paymentDA.GetOutstandingPaymentsAsync();
                var overdue = await _paymentDA.GetOverduePaymentsAsync(30);

                var dashboard = new
                {
                    Period = new { From = from, To = to },
                    Summary = summary,
                    OutstandingPaymentsCount = outstanding.Count,
                    OverduePaymentsCount = overdue.Count,
                    RecentPayments = await _paymentDA.GetPaymentsByDateRangeAsync(
                        DateTime.Now.AddDays(-7), DateTime.Now)
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #region Private Helper Methods

        private string GetPaymentModeLabel(PaymentMode mode)
        {
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
                _ => "Inconnu"
            };
        }

        private string GetPaymentStatusLabel(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "En attente",
                PaymentStatus.Paid => "Payé",
                PaymentStatus.PartiallyPaid => "Partiellement payé",
                PaymentStatus.Overdue => "En retard",
                PaymentStatus.Cancelled => "Annulé",
                _ => "Inconnu"
            };
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Janvier",
                2 => "Février",
                3 => "Mars",
                4 => "Avril",
                5 => "Mai",
                6 => "Juin",
                7 => "Juillet",
                8 => "Août",
                9 => "Septembre",
                10 => "Octobre",
                11 => "Novembre",
                12 => "Décembre",
                _ => $"Mois {month}"
            };
        }

        #endregion
    }

    #region Request/Response DTOs

    public class UpdatePaymentStatusRequest
    {
        public PaymentStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class PaymentFilterQueryParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
        public int? DossierNumber { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public double? AmountFrom { get; set; }
        public double? AmountTo { get; set; }
        public int? PaymentType { get; set; }
        public PaymentMode? PaymentMode { get; set; }
        public bool? IsPaid { get; set; }
        public bool? HasOutstanding { get; set; }
        public string SortBy { get; set; } = PaymentSortFields.Date;
        public string SortDirection { get; set; } = "Descending";
    }

    #endregion
}