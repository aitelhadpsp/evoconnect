using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EvoConnect.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialController(IFinancialDA _financialDA) : ControllerBase
    {
        #region Invoice Endpoints

        /// <summary>
        /// Get paginated invoices with filters
        /// </summary>
        [HttpPost("invoices/paginated")]
        public async Task<IActionResult> GetPaginatedInvoices([FromBody] InvoiceFilterRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request is required");
            }

            var result = await _financialDA.GetPaginatedInvoicesAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        [HttpGet("invoices/{invoiceId}")]
        public async Task<IActionResult> GetInvoiceById(string invoiceId)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
            {
                return BadRequest("Invoice ID is required");
            }

            var invoice = await _financialDA.GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
            {
                return NotFound($"Invoice with ID {invoiceId} not found");
            }

            return Ok(invoice);
        }

        /// <summary>
        /// Get invoices for a specific patient
        /// </summary>
        [HttpGet("invoices/patient/{patientId}")]
        public async Task<IActionResult> GetInvoicesByPatient(int patientId)
        {
            if (patientId <= 0)
            {
                return BadRequest("Valid patient ID is required");
            }

            var invoices = await _financialDA.GetInvoicesByPatientAsync(patientId);
            return Ok(invoices);
        }

        /// <summary>
        /// Create new invoice
        /// </summary>
        [HttpPost("invoices")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invoice data is required");
            }

            if (string.IsNullOrWhiteSpace(request.InvoiceId))
            {
                return BadRequest("Invoice ID is required");
            }

            if (request.PatientId <= 0)
            {
                return BadRequest("Valid patient ID is required");
            }

            var success = await _financialDA.CreateInvoiceAsync(request);
            if (success)
            {
                return CreatedAtAction(nameof(GetInvoiceById),
                    new { invoiceId = request.InvoiceId },
                    new { message = "Invoice created successfully", invoiceId = request.InvoiceId });
            }

            return StatusCode(500, "Failed to create invoice");
        }

        /// <summary>
        /// Get recent invoices
        /// </summary>
        [HttpGet("invoices/recent")]
        public async Task<IActionResult> GetRecentInvoices([FromQuery] int days = 30, [FromQuery] int limit = 50)
        {
            if (days < 1 || days > 365)
            {
                return BadRequest("Days must be between 1 and 365");
            }

            if (limit < 1 || limit > 200)
            {
                return BadRequest("Limit must be between 1 and 200");
            }

            var invoices = await _financialDA.GetRecentInvoicesAsync(days, limit);
            return Ok(invoices);
        }

        /// <summary>
        /// Get overdue invoices
        /// </summary>
        [HttpGet("invoices/overdue")]
        public async Task<IActionResult> GetOverdueInvoices()
        {
            var invoices = await _financialDA.GetOverdueInvoicesAsync();
            return Ok(invoices);
        }

        #endregion

        #region Quote Endpoints

        /// <summary>
        /// Get paginated quotes with filters
        /// </summary>
        [HttpPost("quotes/paginated")]
        public async Task<IActionResult> GetPaginatedQuotes([FromBody] QuoteFilterRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request is required");
            }

            var result = await _financialDA.GetPaginatedQuotesAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Get quote by ID
        /// </summary>
        [HttpGet("quotes/{quoteId:int}")]
        public async Task<IActionResult> GetQuoteById(int quoteId)
        {
            if (quoteId <= 0)
            {
                return BadRequest("Valid quote ID is required");
            }

            var quote = await _financialDA.GetQuoteByIdAsync(quoteId);
            if (quote == null)
            {
                return NotFound($"Quote with ID {quoteId} not found");
            }

            return Ok(quote);
        }

        /// <summary>
        /// Get quotes for a specific patient
        /// </summary>
        [HttpGet("quotes/patient/{patientId:int}")]
        public async Task<IActionResult> GetQuotesByPatient(int patientId)
        {
            if (patientId <= 0)
            {
                return BadRequest("Valid patient ID is required");
            }

            var quotes = await _financialDA.GetQuotesByPatientAsync(patientId);
            return Ok(quotes);
        }

        /// <summary>
        /// Create new quote
        /// </summary>
        [HttpPost("quotes")]
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequest request)
        {
            if (request == null)
            {
                return BadRequest("Quote data is required");
            }

            if (string.IsNullOrWhiteSpace(request.QuoteName))
            {
                return BadRequest("Quote name is required");
            }

            if (request.PatientId <= 0)
            {
                return BadRequest("Valid patient ID is required");
            }

            var quoteId = await _financialDA.CreateQuoteAsync(request);
            if (quoteId > 0)
            {
                return CreatedAtAction(nameof(GetQuoteById),
                    new { quoteId = quoteId },
                    new { message = "Quote created successfully", quoteId = quoteId });
            }

            return StatusCode(500, "Failed to create quote");
        }

        /// <summary>
        /// Update existing quote
        /// </summary>
        [HttpPut("quotes/{quoteId:int}")]
        public async Task<IActionResult> UpdateQuote(int quoteId, [FromBody] UpdateQuoteRequest request)
        {
            if (quoteId <= 0)
            {
                return BadRequest("Valid quote ID is required");
            }

            if (request == null)
            {
                return BadRequest("Quote data is required");
            }

            var success = await _financialDA.UpdateQuoteAsync(quoteId, request);
            if (success)
            {
                return Ok(new { message = "Quote updated successfully" });
            }

            return NotFound($"Quote with ID {quoteId} not found or update failed");
        }

        /// <summary>
        /// Update quote acceptance status
        /// </summary>
        [HttpPut("quotes/{quoteId:int}/acceptance")]
        public async Task<IActionResult> UpdateQuoteAcceptance(int quoteId, [FromBody] QuoteAcceptanceRequest request)
        {
            if (quoteId <= 0)
            {
                return BadRequest("Valid quote ID is required");
            }

            if (request == null)
            {
                return BadRequest("Acceptance data is required");
            }

            var success = await _financialDA.UpdateQuoteAcceptanceAsync(quoteId, request.IsAccepted);
            if (success)
            {
                return Ok(new { message = $"Quote {(request.IsAccepted ? "accepted" : "rejected")} successfully" });
            }

            return NotFound($"Quote with ID {quoteId} not found or update failed");
        }

        /// <summary>
        /// Get pending quotes
        /// </summary>
        [HttpGet("quotes/pending")]
        public async Task<IActionResult> GetPendingQuotes()
        {
            var quotes = await _financialDA.GetPendingQuotesAsync();
            return Ok(quotes);
        }

        #endregion

        #region Insurance Endpoints

        /// <summary>
        /// Get all insurance companies
        /// </summary>
        [HttpGet("insurance")]
        public async Task<IActionResult> GetAllInsuranceCompanies()
        {
            var insurances = await _financialDA.GetAllInsuranceCompaniesAsync();
            return Ok(insurances);
        }

        /// <summary>
        /// Get insurance company by ID
        /// </summary>
        [HttpGet("insurance/{insuranceId:int}")]
        public async Task<IActionResult> GetInsuranceById(int insuranceId)
        {
            if (insuranceId <= 0)
            {
                return BadRequest("Valid insurance ID is required");
            }

            var insurance = await _financialDA.GetInsuranceByIdAsync(insuranceId);
            if (insurance == null)
            {
                return NotFound($"Insurance company with ID {insuranceId} not found");
            }

            return Ok(insurance);
        }

        #endregion

        #region Financial Statistics and Reporting

        /// <summary>
        /// Get financial statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetFinancialStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var statistics = await _financialDA.GetFinancialStatisticsAsync(fromDate, toDate);
            return Ok(statistics);
        }

        /// <summary>
        /// Get monthly revenue for a specific year
        /// </summary>
        [HttpGet("revenue/monthly/{year:int}")]
        public async Task<IActionResult> GetMonthlyRevenue(int year)
        {
            if (year < 2000 || year > DateTime.Now.Year + 1)
            {
                return BadRequest("Invalid year specified");
            }

            var monthlyRevenue = await _financialDA.GetMonthlyRevenueAsync(year);
            return Ok(monthlyRevenue);
        }

        /// <summary>
        /// Get patient financial balance
        /// </summary>
        [HttpGet("balance/patient/{patientId:int}")]
        public async Task<IActionResult> GetPatientBalance(int patientId)
        {
            if (patientId <= 0)
            {
                return BadRequest("Valid patient ID is required");
            }

            var balance = await _financialDA.GetPatientBalanceAsync(patientId);
            return Ok(new { patientId = patientId, balance = balance });
        }

        #endregion

        #region Advanced Query Endpoints

        /// <summary>
        /// Get financial summary for dashboard
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetFinancialDashboard()
        {
            try
            {
                // Get various metrics for dashboard
                var currentMonthStats = await _financialDA.GetFinancialStatisticsAsync(
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    DateTime.Now
                );

                var recentInvoices = await _financialDA.GetRecentInvoicesAsync(7, 10);
                var pendingQuotes = await _financialDA.GetPendingQuotesAsync();
                var overdueInvoices = await _financialDA.GetOverdueInvoicesAsync();

                var dashboard = new
                {
                    CurrentMonth = currentMonthStats,
                    RecentInvoices = recentInvoices,
                    PendingQuotes = pendingQuotes.Take(10),
                    OverdueInvoices = overdueInvoices.Take(10),
                    Summary = new
                    {
                        PendingQuotesCount = pendingQuotes.Count,
                        OverdueInvoicesCount = overdueInvoices.Count,
                        RecentInvoicesCount = recentInvoices.Count
                    }
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to load dashboard data: {ex.Message}");
            }
        }

        /// <summary>
        /// Search invoices and quotes by text
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchFinancialRecords(
            [FromQuery] string searchText,
            [FromQuery] string type = "all", // "invoices", "quotes", "all"
            [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return BadRequest("Search text is required");
            }

            if (limit < 1 || limit > 100)
            {
                return BadRequest("Limit must be between 1 and 100");
            }

            try
            {
                var results = new
                {
                    SearchText = searchText,
                    Invoices = new List<InvoiceDto>(),
                    Quotes = new List<QuoteDto>()
                };

                if (type == "invoices" || type == "all")
                {
                    var invoiceRequest = new InvoiceFilterRequest
                    {
                        Filters = new InvoiceFilters { InvoiceId = searchText },
                        Pagination = new PaginationRequest { PageNumber = 1, PageSize = limit }
                    };
                    var invoiceResults = await _financialDA.GetPaginatedInvoicesAsync(invoiceRequest);
                    results = new { results.SearchText, Invoices = invoiceResults.Items, results.Quotes };
                }

                if (type == "quotes" || type == "all")
                {
                    var quoteRequest = new QuoteFilterRequest
                    {
                        Pagination = new PaginationRequest { PageNumber = 1, PageSize = limit }
                    };
                    var quoteResults = await _financialDA.GetPaginatedQuotesAsync(quoteRequest);
                    results = new { results.SearchText, results.Invoices, Quotes = quoteResults.Items };
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Search failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Export financial data
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportFinancialData([FromBody] FinancialExportRequest request)
        {
            if (request == null)
            {
                return BadRequest("Export request is required");
            }

            try
            {
                var exportData = new
                {
                    ExportDate = DateTime.Now,
                    Filters = request,
                    Invoices = new List<InvoiceDto>(),
                    Quotes = new List<QuoteDto>(),
                    Statistics = (FinancialStatistics)null
                };

                if (request.IncludeInvoices)
                {
                    var invoiceRequest = new InvoiceFilterRequest
                    {
                        Filters = new InvoiceFilters
                        {
                            DateFrom = request.DateFrom,
                            DateTo = request.DateTo,
                            PatientId = request.PatientId
                        },
                        Pagination = new PaginationRequest { PageNumber = 1, PageSize = 1000 }
                    };
                    var invoiceResults = await _financialDA.GetPaginatedInvoicesAsync(invoiceRequest);
                    exportData = new { exportData.ExportDate, exportData.Filters, Invoices = invoiceResults.Items, exportData.Quotes, exportData.Statistics };
                }

                if (request.IncludeQuotes)
                {
                    var quoteRequest = new QuoteFilterRequest
                    {
                        Filters = new QuoteFilters
                        {
                            CreationDateFrom = request.DateFrom,
                            CreationDateTo = request.DateTo,
                            PatientId = request.PatientId
                        },
                        Pagination = new PaginationRequest { PageNumber = 1, PageSize = 1000 }
                    };
                    var quoteResults = await _financialDA.GetPaginatedQuotesAsync(quoteRequest);
                    exportData = new { exportData.ExportDate, exportData.Filters, exportData.Invoices, Quotes = quoteResults.Items, exportData.Statistics };
                }

                if (request.IncludeStatistics)
                {
                    var statistics = await _financialDA.GetFinancialStatisticsAsync(request.DateFrom, request.DateTo);
                    exportData = new { exportData.ExportDate, exportData.Filters, exportData.Invoices, exportData.Quotes, Statistics = statistics };
                }

                return Ok(exportData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Export failed: {ex.Message}");
            }
        }

        #endregion
        [HttpGet("devis/statistics")]
        public async Task<IActionResult> GetDevisStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-3);
            var end = endDate ?? DateTime.Now;

            if (start > end)
            {
                return BadRequest("Start date cannot be after end date");
            }

            var stats = await _financialDA.GetDevisStatsAsync(start, end);
            return Ok(stats);
        }

        /// <summary>
        /// Get monthly devis statistics
        /// </summary>
        [HttpGet("devis/statistics/monthly/{year:int}")]
        public async Task<IActionResult> GetMonthlyDevisStats(int year)
        {
            if (year < 2000 || year > DateTime.Now.Year + 1)
            {
                return BadRequest("Invalid year specified");
            }

            var stats = await _financialDA.GetMonthlyDevisStatsAsync(year);
            return Ok(stats);
        }

        /// <summary>
        /// Get daily devis statistics
        /// </summary>
        [HttpGet("devis/statistics/daily")]
        public async Task<IActionResult> GetDailyDevisStats(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-1);
            var to = toDate ?? DateTime.Now;

            if (from > to)
            {
                return BadRequest("From date cannot be after to date");
            }

            var stats = await _financialDA.GetDailyDevisStatsAsync(from, to);
            return Ok(stats);
        }

        /// <summary>
        /// Get devis summary statistics
        /// </summary>
        [HttpGet("devis/statistics/summary")]
        public async Task<IActionResult> GetDevisSummaryStats(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-3);
            var to = toDate ?? DateTime.Now;

            if (from > to)
            {
                return BadRequest("From date cannot be after to date");
            }

            var summary = await _financialDA.GetDevisSummaryStatsAsync(from, to);
            return Ok(summary);
        }

        /// <summary>
        /// Get devis statistics by state
        /// </summary>
        [HttpGet("devis/statistics/by-state")]
        public async Task<IActionResult> GetDevisByState(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var from = fromDate ?? DateTime.Now.AddMonths(-3);
            var to = toDate ?? DateTime.Now;

            if (from > to)
            {
                return BadRequest("From date cannot be after to date");
            }

            var stats = await _financialDA.GetDevisByStateAsync(from, to);
            return Ok(stats);
        }

        /// <summary>
        /// Get comprehensive devis dashboard
        /// </summary>
        [HttpGet("devis/dashboard")]
        public async Task<IActionResult> GetDevisDashboard()
        {
            try
            {
                var now = DateTime.Now;
                var threeMonthsAgo = now.AddMonths(-3);

                // Get multiple metrics
                var summary = await _financialDA.GetDevisSummaryStatsAsync(threeMonthsAgo, now);
                var monthlyStats = await _financialDA.GetMonthlyDevisStatsAsync(now.Year);
                var stateStats = await _financialDA.GetDevisByStateAsync(threeMonthsAgo, now);
                var recentStats = await _financialDA.GetDailyDevisStatsAsync(now.AddDays(-30), now);

                var dashboard = new
                {
                    Summary = summary,
                    MonthlyBreakdown = monthlyStats,
                    ByState = stateStats,
                    Last30Days = recentStats,
                    Period = new
                    {
                        From = threeMonthsAgo,
                        To = now
                    }
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to load devis dashboard: {ex.Message}");
            }
        }
    }

    // Additional request DTOs
    public class QuoteAcceptanceRequest
    {
        public bool IsAccepted { get; set; }
    }

    public class FinancialExportRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? PatientId { get; set; }
        public bool IncludeInvoices { get; set; } = true;
        public bool IncludeQuotes { get; set; } = true;
        public bool IncludeStatistics { get; set; } = true;
        public string Format { get; set; } = "json"; // json, csv, excel
    }
}