using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EvoConnect.Server.Data;
using EvoConnect.Server.Models;
using EvoConnect.Server.Repository.Interfaces;

namespace EvoConnect.Server.Repository
{
    public class FinancialDA : IFinancialDA
    {
        private readonly ClinicDbContext _context;

        public FinancialDA(ClinicDbContext context)
        {
            _context = context;
        }

        #region Invoice Methods 

        /// <summary>
        /// Get paginated invoices with filters
        /// </summary>
        public async Task<PaginatedResult<InvoiceDto>> GetPaginatedInvoicesAsync(InvoiceFilterRequest request)
        {
            try
            {
                var validatedRequest = ValidateInvoiceRequest(request);

                // Base query with relations
                var query = BuildInvoiceBaseQuery(validatedRequest.Filters);

                // Get total count
                int totalCount = await query.CountAsync();

                // Apply pagination and sorting
                var pagedQuery = ApplyInvoicePaginationAndSorting(query, validatedRequest);

                // Execute and map to DTOs
                var factures = await pagedQuery.ToListAsync();
                var invoices = factures.Select(MapToInvoiceDto).ToList();

                var metadata = CreatePaginationMetadata(validatedRequest.Pagination, totalCount);

                return new PaginatedResult<InvoiceDto>
                {
                    Items = invoices,
                    Metadata = metadata
                };
            }
            catch (Exception)
            {
                return new PaginatedResult<InvoiceDto>
                {
                    Items = new List<InvoiceDto>(),
                    Metadata = new PaginationMetadata()
                };
            }
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        public async Task<InvoiceDto?> GetInvoiceByIdAsync(string invoiceId)
        {
            try
            {
                var facture = await _context.Factures
                    .Include(f => f.Personne)
                    .FirstOrDefaultAsync(f => f.IdFacture == invoiceId);

                return facture != null ? MapToInvoiceDto(facture) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get invoices by patient ID
        /// </summary>
        public async Task<List<InvoiceDto>> GetInvoicesByPatientAsync(int patientId)
        {
            try
            {
                var factures = await _context.Factures
                    .Include(f => f.Personne)
                    .Where(f => f.IdPersonne == patientId)
                    .OrderByDescending(f => f.FactDate)
                    .ToListAsync();

                return factures.Select(MapToInvoiceDto).ToList();
            }
            catch (Exception)
            {
                return new List<InvoiceDto>();
            }
        }

        /// <summary>
        /// Get recent invoices
        /// </summary>
        public async Task<List<InvoiceDto>> GetRecentInvoicesAsync(int days = 30, int limit = 50)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-days);

                var factures = await _context.Factures
                    .Include(f => f.Personne)
                    .Where(f => f.FactDate >= cutoffDate)
                    .OrderByDescending(f => f.FactDate)
                    .Take(limit)
                    .ToListAsync();

                return factures.Select(MapToInvoiceDto).ToList();
            }
            catch (Exception)
            {
                return new List<InvoiceDto>();
            }
        }

        /// <summary>
        /// Get overdue invoices
        /// </summary>
        public async Task<List<InvoiceDto>> GetOverdueInvoicesAsync()
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-30);

                var factures = await _context.Factures
                    .Include(f => f.Personne)
                    .Where(f => f.FactPartPatient > 0 && f.FactDate < cutoffDate)
                    .OrderBy(f => f.FactDate)
                    .ToListAsync();

                return factures.Select(MapToInvoiceDto).ToList();
            }
            catch (Exception)
            {
                return new List<InvoiceDto>();
            }
        }

        /// <summary>
        /// Create new invoice
        /// </summary>
        public async Task<bool> CreateInvoiceAsync(CreateInvoiceRequest request)
        {
            try
            {
                var facture = new Facture
                {
                    IdFacture = request.InvoiceId,
                    IdPersonne = request.PatientId,
                    IdUtil = request.UserId,
                    FactMontant = request.Amount,
                    FactDate = request.InvoiceDate,
                    FactPartPatient = request.PatientPortion
                };

                _context.Factures.Add(facture);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Quote Methods

        /// <summary>
        /// Get paginated quotes with filters
        /// </summary>
        public async Task<PaginatedResult<QuoteDto>> GetPaginatedQuotesAsync(QuoteFilterRequest request)
        {
            try
            {
                var validatedRequest = ValidateQuoteRequest(request);

                var query = BuildQuoteBaseQuery(validatedRequest.Filters);
                int totalCount = await query.CountAsync();

                var pagedQuery = ApplyQuotePaginationAndSorting(query, validatedRequest);
                var devis = await pagedQuery.ToListAsync();
                var quotes = devis.Select(MapToQuoteDto).ToList();

                var metadata = CreatePaginationMetadata(validatedRequest.Pagination, totalCount);

                return new PaginatedResult<QuoteDto>
                {
                    Items = quotes,
                    Metadata = metadata
                };
            }
            catch (Exception)
            {
                return new PaginatedResult<QuoteDto>
                {
                    Items = new List<QuoteDto>(),
                    Metadata = new PaginationMetadata()
                };
            }
        }

        /// <summary>
        /// Get quote by ID
        /// </summary>
        public async Task<QuoteDto?> GetQuoteByIdAsync(int quoteId)
        {
            try
            {
                var devis = await _context.DentalisDevis
                    .Include(d => d.Patient)
                        .ThenInclude(p => p.Personne)
                    .FirstOrDefaultAsync(d => d.DevisPk == quoteId);

                return devis != null ? MapToQuoteDto(devis) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get quotes by patient ID
        /// </summary>
        public async Task<List<QuoteDto>> GetQuotesByPatientAsync(int patientId)
        {
            try
            {
                var devis = await _context.DentalisDevis
                    .Include(d => d.Patient)
                        .ThenInclude(p => p.Personne)
                    .Where(d => d.DevisIdPatient == patientId)
                    .OrderByDescending(d => d.DevisDateCreat)
                    .ToListAsync();

                return devis.Select(MapToQuoteDto).ToList();
            }
            catch (Exception)
            {
                return new List<QuoteDto>();
            }
        }

        /// <summary>
        /// Get pending quotes (not accepted yet)
        /// </summary>
        public async Task<List<QuoteDto>> GetPendingQuotesAsync()
        {
            try
            {
                var devis = await _context.DentalisDevis
                    .Include(d => d.Patient)
                        .ThenInclude(p => p.Personne)
                    .Where(d => d.DevisAccepte == 0)
                    .OrderByDescending(d => d.DevisDateCreat)
                    .ToListAsync();

                return devis.Select(MapToQuoteDto).ToList();
            }
            catch (Exception)
            {
                return new List<QuoteDto>();
            }
        }

        /// <summary>
        /// Create new quote
        /// </summary>
        public async Task<int> CreateQuoteAsync(CreateQuoteRequest request)
        {
            try
            {
                var now = DateTime.Now;
                var devis = new DentalisDevis
                {
                    DevisNom = request.QuoteName,
                    DevisIdPatient = request.PatientId,
                    DevisMontant = request.Amount,
                    DevisDateCreat = now,
                    DevisDateMaj = now,
                    DevisMontantAvRemise = request.AmountBeforeDiscount,
                    DevisRemise = request.Discount,
                    DevisRemiseType = (short)request.DiscountType
                };

                _context.DentalisDevis.Add(devis);
                await _context.SaveChangesAsync();
                return devis.DevisPk;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Update quote
        /// </summary>
        public async Task<bool> UpdateQuoteAsync(int quoteId, UpdateQuoteRequest request)
        {
            try
            {
                var devis = await _context.DentalisDevis
                    .FirstOrDefaultAsync(d => d.DevisPk == quoteId);

                if (devis == null)
                    return false;

                devis.DevisNom = request.QuoteName;
                devis.DevisMontant = request.Amount;
                devis.DevisDateMaj = DateTime.Now;
                devis.DevisMontantAvRemise = request.AmountBeforeDiscount;
                devis.DevisRemise = request.Discount;
                devis.DevisRemiseType = (short)request.DiscountType;
                devis.DevisModeReg = request.PaymentMode ?? "";

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Update quote acceptance status
        /// </summary>
        public async Task<bool> UpdateQuoteAcceptanceAsync(int quoteId, bool accepted)
        {
            try
            {
                var devis = await _context.DentalisDevis
                    .FirstOrDefaultAsync(d => d.DevisPk == quoteId);

                if (devis == null)
                    return false;

                devis.DevisAccepte = (short)(accepted ? 1 : 0);
                devis.DevisDateMaj = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Insurance Methods

        /// <summary>
        /// Get all insurance companies
        /// </summary>
        public async Task<List<InsuranceDto>> GetAllInsuranceCompaniesAsync()
        {
            try
            {
                var mutuelles = await _context.Mutuelles
                    .OrderBy(m => m.MutuelleNom)
                    .ToListAsync();

                return mutuelles.Select(MapToInsuranceDto).ToList();
            }
            catch (Exception)
            {
                return new List<InsuranceDto>();
            }
        }

        /// <summary>
        /// Get insurance by ID
        /// </summary>
        public async Task<InsuranceDto?> GetInsuranceByIdAsync(int insuranceId)
        {
            try
            {
                var mutuelle = await _context.Mutuelles
                    .FirstOrDefaultAsync(m => m.IdMutuelle == insuranceId);

                return mutuelle != null ? MapToInsuranceDto(mutuelle) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region Financial Statistics

        /// <summary>
        /// Get patient financial balance
        /// </summary>
        public async Task<float> GetPatientBalanceAsync(int patientId)
        {
            try
            {
                // Get balance from Patient table
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.IdPersonne == patientId);

                float balance = (float)(patient?.PatSolde ?? 0);

                // Add recent invoices
                var oneYearAgo = DateTime.Now.AddYears(-1);
                var recentInvoicesBalance = await _context.Factures
                    .Where(f => f.IdPersonne == patientId && f.FactDate >= oneYearAgo)
                    .SumAsync(f => (float)(f.FactPartPatient));

                return balance + recentInvoicesBalance;
            }
            catch (Exception)
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// Get monthly revenue summary
        /// </summary>
        public async Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync(int year)
        {
            try
            {
                var monthlyData = await _context.Factures
                    .Where(f => f.FactDate.Year == year)
                    .GroupBy(f => f.FactDate.Month)
                    .Select(g => new MonthlyRevenue
                    {
                        Month = g.Key,
                        Year = year,
                        InvoiceCount = g.Count(),
                        TotalRevenue = g.Sum(f => (float)(f.FactMontant ?? 0)),
                        PatientPortion = g.Sum(f => (float)f.FactPartPatient)
                    })
                    .OrderBy(m => m.Month)
                    .ToListAsync();

                return monthlyData;
            }
            catch (Exception)
            {
                return new List<MonthlyRevenue>();
            }
        }

        /// <summary>
        /// Get financial statistics
        /// </summary>
        public async Task<FinancialStatistics> GetFinancialStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var invoiceQuery = _context.Factures.AsQueryable();
                var quoteQuery = _context.DentalisDevis.AsQueryable();

                if (fromDate.HasValue && toDate.HasValue)
                {
                    invoiceQuery = invoiceQuery.Where(f => f.FactDate >= fromDate.Value && f.FactDate <= toDate.Value);
                    quoteQuery = quoteQuery.Where(d => d.DevisDateCreat >= fromDate.Value && d.DevisDateCreat <= toDate.Value);
                }

                var invoiceStats = await invoiceQuery
                    .GroupBy(f => 1)
                    .Select(g => new
                    {
                        TotalInvoices = g.Count(),
                        TotalRevenue = g.Sum(f => (float)(f.FactMontant ?? 0)),
                        AverageInvoiceAmount = g.Average(f => (float)(f.FactMontant ?? 0)),
                        TotalPatientPortion = g.Sum(f => (float)f.FactPartPatient)
                    })
                    .FirstOrDefaultAsync();

                var quoteStats = await quoteQuery
                    .GroupBy(d => 1)
                    .Select(g => new
                    {
                        AcceptedQuotes = g.Count(d => d.DevisAccepte == 1),
                        PendingQuotes = g.Count(d => d.DevisAccepte == 0)
                    })
                    .FirstOrDefaultAsync();

                return new FinancialStatistics
                {
                    TotalInvoices = invoiceStats?.TotalInvoices ?? 0,
                    TotalRevenue = invoiceStats?.TotalRevenue ?? 0,
                    AverageInvoiceAmount = invoiceStats?.AverageInvoiceAmount ?? 0,
                    TotalPatientPortion = invoiceStats?.TotalPatientPortion ?? 0,
                    AcceptedQuotes = quoteStats?.AcceptedQuotes ?? 0,
                    PendingQuotes = quoteStats?.PendingQuotes ?? 0,
                    PeriodFrom = fromDate,
                    PeriodTo = toDate
                };
            }
            catch (Exception)
            {
                return new FinancialStatistics();
            }
        }

        #endregion

        #region Helper Methods

        private IQueryable<Facture> BuildInvoiceBaseQuery(InvoiceFilters filters)
        {
            var query = _context.Factures
                .Include(f => f.Personne)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filters.InvoiceId))
                query = query.Where(f => f.IdFacture.ToString().Contains(filters.InvoiceId));

            if (filters.PatientId.HasValue)
                query = query.Where(f => f.IdPersonne == filters.PatientId.Value);

            if (filters.DateFrom.HasValue)
                query = query.Where(f => f.FactDate >= filters.DateFrom.Value);

            if (filters.DateTo.HasValue)
                query = query.Where(f => f.FactDate <= filters.DateTo.Value);

            if (filters.AmountFrom.HasValue)
                query = query.Where(f => f.FactMontant >= filters.AmountFrom.Value);

            if (filters.AmountTo.HasValue)
                query = query.Where(f => f.FactMontant <= filters.AmountTo.Value);

            return query;
        }

        private IQueryable<DentalisDevis> BuildQuoteBaseQuery(QuoteFilters filters)
        {
            var query = _context.DentalisDevis
                .Include(d => d.Patient)
                    .ThenInclude(p => p.Personne)
                .AsQueryable();

            // Apply filters
            if (filters.PatientId.HasValue)
                query = query.Where(d => d.DevisIdPatient == filters.PatientId.Value);

            if (filters.IsAccepted.HasValue)
                query = query.Where(d => d.DevisAccepte == (filters.IsAccepted.Value ? 1 : 0));

            if (filters.IsPresented.HasValue)
                query = query.Where(d => d.DevisPresente == (filters.IsPresented.Value ? 1 : 0));

            if (filters.CreationDateFrom.HasValue)
                query = query.Where(d => d.DevisDateCreat >= filters.CreationDateFrom.Value);

            if (filters.CreationDateTo.HasValue)
                query = query.Where(d => d.DevisDateCreat <= filters.CreationDateTo.Value);

            return query;
        }

        private IQueryable<Facture> ApplyInvoicePaginationAndSorting(IQueryable<Facture> query, InvoiceFilterRequest request)
        {
            // Apply sorting
            query = request.Sorting.SortBy switch
            {
                "InvoiceDate" => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(f => f.FactDate)
                    : query.OrderBy(f => f.FactDate),
                "Amount" => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(f => f.FactMontant)
                    : query.OrderBy(f => f.FactMontant),
                "PatientName" => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(f => f.Personne.PerNom)
                    : query.OrderBy(f => f.Personne.PerNom),
                _ => query.OrderByDescending(f => f.FactDate)
            };

            // Apply pagination
            if (request.Pagination.ValidatedPageSize > 0 && request.Pagination.ValidatedPageNumber > 0)
            {
                var skip = (request.Pagination.ValidatedPageNumber - 1) * request.Pagination.ValidatedPageSize;
                query = query.Skip(skip).Take(request.Pagination.ValidatedPageSize);
            }

            return query;
        }

        private IQueryable<DentalisDevis> ApplyQuotePaginationAndSorting(IQueryable<DentalisDevis> query, QuoteFilterRequest request)
        {
            // Apply sorting
            query = request.Sorting.SortBy switch
            {
                "CreationDate" => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(d => d.DevisDateCreat)
                    : query.OrderBy(d => d.DevisDateCreat),
                "Amount" => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(d => d.DevisMontant)
                    : query.OrderBy(d => d.DevisMontant),
                "PatientName" => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(d => d.Patient.Personne.PerNom)
                    : query.OrderBy(d => d.Patient.Personne.PerNom),
                "IsAccepted" => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(d => d.DevisAccepte)
                    : query.OrderBy(d => d.DevisAccepte),
                _ => query.OrderByDescending(d => d.DevisDateCreat)
            };

            // Apply pagination
            if (request.Pagination.ValidatedPageSize > 0 && request.Pagination.ValidatedPageNumber > 0)
            {
                var skip = (request.Pagination.ValidatedPageNumber - 1) * request.Pagination.ValidatedPageSize;
                query = query.Skip(skip).Take(request.Pagination.ValidatedPageSize);
            }

            return query;
        }

        private InvoiceDto MapToInvoiceDto(Facture facture)
        {
            return new InvoiceDto
            {
                InvoiceId = facture.IdFacture,
                PatientId = facture.IdPersonne,
                UserId = facture.IdUtil,
                Amount = (float)(facture.FactMontant ?? 0),
                InvoiceDate = facture.FactDate,
                PatientPortion = (float)facture.FactPartPatient,
                PatientLastName = facture.Personne?.PerNom?.Trim() ?? "",
                PatientFirstName = facture.Personne?.PerPrenom?.Trim() ?? ""
            };
        }

        private QuoteDto MapToQuoteDto(DentalisDevis devis)
        {
            return new QuoteDto
            {
                QuoteId = devis.DevisPk,
                QuoteName = devis.DevisNom?.Trim() ?? "",
                PatientId = devis.DevisIdPatient,
                IsAccepted = devis.DevisAccepte == 1,
                Amount = (float)devis.DevisMontant,
                CreationDate = devis.DevisDateCreat,
                LastModifiedDate = devis.DevisDateMaj,
                PrintDate = devis.DevisDateImpr,
                IsPresented = devis.DevisPresente == 1,
                PresentationDate = devis.DevisDatePresent,
                InvoiceNumber = devis.DevisNumFacture?.Trim() ?? "",
                PaymentMode = devis.DevisModeReg?.Trim() ?? "",
                Discount = (float)(devis.DevisRemise ?? 0),
                AmountBeforeDiscount = (float)(devis.DevisMontantAvRemise ?? 0),
                DiscountType = devis.DevisRemiseType,
                PatientLastName = devis.Patient?.Personne?.PerNom?.Trim() ?? "",
                PatientFirstName = devis.Patient?.Personne?.PerPrenom?.Trim() ?? ""
            };
        }

        private InsuranceDto MapToInsuranceDto(Mutuelle mutuelle)
        {
            return new InsuranceDto
            {
                InsuranceId = mutuelle.IdMutuelle,
                AddressId = mutuelle.IdAdresse,
                Name = mutuelle.MutuelleNom?.Trim() ?? "",
                Phone = mutuelle.MutuelleTel?.Trim() ?? "",
                Number = mutuelle.MutuelleNumero?.Trim() ?? ""
            };
        }

        private InvoiceFilterRequest ValidateInvoiceRequest(InvoiceFilterRequest request)
        {
            request ??= new InvoiceFilterRequest();
            request.Filters ??= new InvoiceFilters();
            request.Pagination ??= new PaginationRequest();
            request.Sorting ??= new SortingRequest();
            return request;
        }

        private QuoteFilterRequest ValidateQuoteRequest(QuoteFilterRequest request)
        {
            request ??= new QuoteFilterRequest();
            request.Filters ??= new QuoteFilters();
            request.Pagination ??= new PaginationRequest();
            request.Sorting ??= new SortingRequest();
            return request;
        }

        private PaginationMetadata CreatePaginationMetadata(PaginationRequest pagination, int totalItems)
        {
            var totalPages = (int)Math.Ceiling((double)totalItems / pagination.ValidatedPageSize);

            return new PaginationMetadata
            {
                CurrentPage = pagination.ValidatedPageNumber,
                PageSize = pagination.ValidatedPageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNext = pagination.ValidatedPageNumber < totalPages,
                HasPrevious = pagination.ValidatedPageNumber > 1
            };
        }

        #endregion

        #region Additional Business Methods

        /// <summary>
        /// Get invoice summary for a patient
        /// </summary>
        public async Task<PatientInvoiceSummary> GetPatientInvoiceSummaryAsync(int patientId)
        {
            try
            {
                var summary = await _context.Factures
                    .Where(f => f.IdPersonne == patientId)
                    .GroupBy(f => f.IdPersonne)
                    .Select(g => new PatientInvoiceSummary
                    {
                        PatientId = patientId,
                        TotalInvoices = g.Count(),
                        TotalAmount = g.Sum(f => (double)(f.FactMontant ?? 0)),
                        TotalPatientPortion = g.Sum(f => (double)f.FactPartPatient),
                        LastInvoiceDate = g.Max(f => f.FactDate),
                        AverageAmount = g.Average(f => (double)(f.FactMontant ?? 0))
                    })
                    .FirstOrDefaultAsync();

                return summary ?? new PatientInvoiceSummary { PatientId = patientId };
            }
            catch (Exception)
            {
                return new PatientInvoiceSummary { PatientId = patientId };
            }
        }

        /// <summary>
        /// Get quote summary for a patient
        /// </summary>
        public async Task<PatientQuoteSummary> GetPatientQuoteSummaryAsync(int patientId)
        {
            try
            {
                var summary = await _context.DentalisDevis
                    .Where(d => d.DevisIdPatient == patientId)
                    .GroupBy(d => d.DevisIdPatient)
                    .Select(g => new PatientQuoteSummary
                    {
                        PatientId = patientId,
                        TotalQuotes = g.Count(),
                        AcceptedQuotes = g.Count(d => d.DevisAccepte == 1),
                        PendingQuotes = g.Count(d => d.DevisAccepte == 0),
                        TotalAmount = g.Sum(d => (double)d.DevisMontant),
                        LastQuoteDate = g.Max(d => d.DevisDateCreat)
                    })
                    .FirstOrDefaultAsync();

                return summary ?? new PatientQuoteSummary { PatientId = patientId };
            }
            catch (Exception)
            {
                return new PatientQuoteSummary { PatientId = patientId };
            }
        }

    public async Task<List<DevisStats>> GetDevisStatsAsync(DateTime startDate, DateTime endDate)
        {
            var groupingType = DetermineGroupingType(startDate, endDate);
            var query = _context.DentalisDevis
                .Where(d => d.DevisDateCreat >= startDate && d.DevisDateCreat <= endDate);

            var rawStats = groupingType switch
            {
                GroupingType.Hour => await GroupDevisByHour(query),
                GroupingType.Day => await GroupDevisByDay(query),
                GroupingType.Month => await GroupDevisByMonth(query),
                GroupingType.Year => await GroupDevisByYear(query),
                _ => await GroupDevisByYear(query)
            };

            return FillDevisGaps(rawStats, startDate, endDate, groupingType);
        }


        public async Task<List<MonthlyDevisStats>> GetMonthlyDevisStatsAsync(int year)
{
    return await _context.DentalisDevis
        .Where(d => d.DevisDateCreat.Year == year)
        .GroupBy(d => new { d.DevisDateCreat.Year, d.DevisDateCreat.Month })
        .Select(g => new MonthlyDevisStats
        {
            Year = g.Key.Year,
            Month = g.Key.Month,
            TotalDevis = g.Count(),
            AcceptedDevis = g.Count(x => x.DevisAccepte == 1),
            RefusedDevis = g.Count(x => x.DevisAccepte == 2),
            PendingDevis = g.Count(x => x.DevisAccepte == 0),
            AverageResponseTime = g.Where(x => x.DevisAccepte == 1 || x.DevisAccepte == 2)
                .Average(x =>( x.DevisDateMaj - x.DevisDateCreat).TotalDays)
        })
        .OrderBy(x => x.Month)
        .ToListAsync();
}

        /// <summary>
        /// Get daily devis statistics
        /// </summary>
        public async Task<List<DailyDevisStats>> GetDailyDevisStatsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.DentalisDevis
                .Where(d => d.DevisDateCreat >= fromDate && d.DevisDateCreat <= toDate)
                .GroupBy(d => d.DevisDateCreat.Date)
                .Select(g => new DailyDevisStats
                {
                    Date = g.Key,
                    TotalDevis = g.Count(),
                    AcceptedDevis = g.Count(x => x.DevisAccepte != 0),
                    RefusedDevis = g.Count(x => x.DevisAccepte != null),
                    PendingDevis = g.Count(x => x.DevisAccepte != 0 && x.DevisAccepte == null)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Get devis summary statistics
        /// </summary>
    public async Task<DevisSummaryStats> GetDevisSummaryStatsAsync(DateTime fromDate, DateTime toDate)
{
    var query = _context.DentalisDevis
        .Where(d => d.DevisDateCreat >= fromDate && d.DevisDateCreat <= toDate);

    return await query.GroupBy(d => 1).Select(g => new DevisSummaryStats
    {
        TotalDevis = g.Count(),
        AcceptedDevis = g.Count(x => x.DevisAccepte == 1),
        RefusedDevis = g.Count(x => x.DevisAccepte == 2),
        PendingDevis = g.Count(x => x.DevisAccepte == 0),
        AcceptanceRate = g.Count() > 0
            ? (double)g.Count(x => x.DevisAccepte == 1) / g.Count() * 100
            : 0,
        RefusalRate = g.Count() > 0
            ? (double)g.Count(x => x.DevisAccepte == 2) / g.Count() * 100
            : 0,
        AverageResponseTime = g.Where(x => x.DevisAccepte == 1 || x.DevisAccepte == 2)
            .Average(x => (x.DevisDateMaj-x.DevisDateCreat ).TotalDays)
    }).FirstOrDefaultAsync() ?? new DevisSummaryStats();
}

     public async Task<List<DevisStateStats>> GetDevisByStateAsync(DateTime fromDate, DateTime toDate)
{
    return await _context.DentalisDevis
        .Where(d => d.DevisDateCreat >= fromDate && d.DevisDateCreat <= toDate)
        .GroupBy(d => d.DevisAccepte)
        .Select(g => new DevisStateStats
        {
            State = g.Key,
            TotalDevis = g.Count(),
            AcceptedDevis = g.Count(x => x.DevisAccepte == 1),
            RefusedDevis = g.Count(x => x.DevisAccepte == 2)
        })
        .OrderByDescending(x => x.TotalDevis)
        .ToListAsync();
}

        #endregion

        #region Devis Grouping Helper Methods

        private async Task<List<DevisStats>> GroupDevisByHour(IQueryable<DentalisDevis> query)
        {
            var results = await query
                .GroupBy(d => new
                {
                    Year = d.DevisDateCreat.Year,
                    Month = d.DevisDateCreat.Month,
                    Day = d.DevisDateCreat.Day,
                    Hour = d.DevisDateCreat.Hour
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    g.Key.Hour,
                    TotalDevis = g.Count(),
                    AcceptedDevis = g.Count(x => x.DevisAccepte !=0),
                    RefusedDevis = g.Count(x => x.DevisAccepte != 1),
                    PendingDevis = g.Count(x => x.DevisAccepte ==0)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day).ThenBy(x => x.Hour)
                .ToListAsync();

            return results.Select(r => new DevisStats
            {
                Year = r.Year,
                Month = r.Month,
                Day = r.Day,
                Hour = r.Hour,
                Period = $"{r.Year:0000}-{r.Month:00}-{r.Day:00} {r.Hour:00}:00",
                TotalDevis = r.TotalDevis,
                AcceptedDevis = r.AcceptedDevis,
                RefusedDevis = r.RefusedDevis,
                PendingDevis = r.PendingDevis,
                GroupingType = GroupingType.Hour
            }).ToList();
        }

        private async Task<List<DevisStats>> GroupDevisByDay(IQueryable<DentalisDevis> query)
        {
            var results = await query
                .GroupBy(d => new
                {
                    Year = d.DevisDateCreat.Year,
                    Month = d.DevisDateCreat.Month,
                    Day = d.DevisDateCreat.Day
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    TotalDevis = g.Count(),
                    AcceptedDevis = g.Count(x => x.DevisAccepte !=0),
                    RefusedDevis = g.Count(x => x.DevisAccepte != null),
                    PendingDevis = g.Count(x => x.DevisAccepte !=0 && x.DevisAccepte == null)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
                .ToListAsync();

            return results.Select(r => new DevisStats
            {
                Year = r.Year,
                Month = r.Month,
                Day = r.Day,
                Period = $"{r.Year:0000}-{r.Month:00}-{r.Day:00}",
                TotalDevis = r.TotalDevis,
                AcceptedDevis = r.AcceptedDevis,
                RefusedDevis = r.RefusedDevis,
                PendingDevis = r.PendingDevis,
                GroupingType = GroupingType.Day
            }).ToList();
        }

        private async Task<List<DevisStats>> GroupDevisByMonth(IQueryable<DentalisDevis> query)
        {
            var results = await query
                .GroupBy(d => new
                {
                    Year = d.DevisDateCreat.Year,
                    Month = d.DevisDateCreat.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalDevis = g.Count(),
                    AcceptedDevis = g.Count(x => x.DevisAccepte !=0),
                    RefusedDevis = g.Count(x => x.DevisAccepte != null),
                    PendingDevis = g.Count(x => x.DevisAccepte !=0 && x.DevisAccepte == null)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return results.Select(r => new DevisStats
            {
                Year = r.Year,
                Month = r.Month,
                Period = $"{r.Year:0000}-{r.Month:00}",
                TotalDevis = r.TotalDevis,
                AcceptedDevis = r.AcceptedDevis,
                RefusedDevis = r.RefusedDevis,
                PendingDevis = r.PendingDevis,
                GroupingType = GroupingType.Month
            }).ToList();
        }

        private async Task<List<DevisStats>> GroupDevisByYear(IQueryable<DentalisDevis> query)
        {
            var results = await query
                .GroupBy(d => d.DevisDateCreat.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalDevis = g.Count(),
                    AcceptedDevis = g.Count(x => x.DevisAccepte !=0),
                    RefusedDevis = g.Count(x => x.DevisAccepte != null),
                    PendingDevis = g.Count(x => x.DevisAccepte !=0 && x.DevisAccepte == null)
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            return results.Select(r => new DevisStats
            {
                Year = r.Year,
                Period = r.Year.ToString(),
                TotalDevis = r.TotalDevis,
                AcceptedDevis = r.AcceptedDevis,
                RefusedDevis = r.RefusedDevis,
                PendingDevis = r.PendingDevis,
                GroupingType = GroupingType.Year
            }).ToList();
        }

        private List<DevisStats> FillDevisGaps(
            List<DevisStats> stats,
            DateTime startDate,
            DateTime endDate,
            GroupingType groupingType)
        {
            var filled = new List<DevisStats>();

            switch (groupingType)
            {
                case GroupingType.Hour:
                    for (var dt = startDate; dt <= endDate; dt = dt.AddHours(1))
                    {
                        var found = stats.FirstOrDefault(s =>
                            s.Year == dt.Year &&
                            s.Month == dt.Month &&
                            s.Day == dt.Day &&
                            s.Hour == dt.Hour);

                        filled.Add(found ?? new DevisStats
                        {
                            Year = dt.Year,
                            Month = dt.Month,
                            Day = dt.Day,
                            Hour = dt.Hour,
                            Period = $"{dt:yyyy-MM-dd HH}:00",
                            TotalDevis = 0,
                            AcceptedDevis = 0,
                            RefusedDevis = 0,
                            PendingDevis = 0,
                            GroupingType = GroupingType.Hour
                        });
                    }
                    break;

                case GroupingType.Day:
                    for (var dt = startDate.Date; dt <= endDate.Date; dt = dt.AddDays(1))
                    {
                        var found = stats.FirstOrDefault(s =>
                            s.Year == dt.Year &&
                            s.Month == dt.Month &&
                            s.Day == dt.Day);

                        filled.Add(found ?? new DevisStats
                        {
                            Year = dt.Year,
                            Month = dt.Month,
                            Day = dt.Day,
                            Period = $"{dt:yyyy-MM-dd}",
                            TotalDevis = 0,
                            AcceptedDevis = 0,
                            RefusedDevis = 0,
                            PendingDevis = 0,
                            GroupingType = GroupingType.Day
                        });
                    }
                    break;

                case GroupingType.Month:
                    var monthStart = new DateTime(startDate.Year, startDate.Month, 1);
                    var monthEnd = new DateTime(endDate.Year, endDate.Month, 1);
                    for (var dt = monthStart; dt <= monthEnd; dt = dt.AddMonths(1))
                    {
                        var found = stats.FirstOrDefault(s =>
                            s.Year == dt.Year &&
                            s.Month == dt.Month);

                        filled.Add(found ?? new DevisStats
                        {
                            Year = dt.Year,
                            Month = dt.Month,
                            Period = $"{dt:yyyy-MM}",
                            TotalDevis = 0,
                            AcceptedDevis = 0,
                            RefusedDevis = 0,
                            PendingDevis = 0,
                            GroupingType = GroupingType.Month
                        });
                    }
                    break;

                case GroupingType.Year:
                    for (int year = startDate.Year; year <= endDate.Year; year++)
                    {
                        var found = stats.FirstOrDefault(s => s.Year == year);

                        filled.Add(found ?? new DevisStats
                        {
                            Year = year,
                            Period = year.ToString(),
                            TotalDevis = 0,
                            AcceptedDevis = 0,
                            RefusedDevis = 0,
                            PendingDevis = 0,
                            GroupingType = GroupingType.Year
                        });
                    }
                    break;
            }

            return filled
                .OrderBy(s => s.Year)
                .ThenBy(s => s.Month)
                .ThenBy(s => s.Day)
                .ThenBy(s => s.Hour)
                .ToList();
        }

        private GroupingType DetermineGroupingType(DateTime startDate, DateTime endDate)
        {
            var dateRange = endDate - startDate;

            return dateRange.TotalDays switch
            {
                <= 1 => GroupingType.Hour,
                <= 90 => GroupingType.Day,
                <= 730 => GroupingType.Month,
                _ => GroupingType.Year
            };
        }

        #endregion
    }
    }