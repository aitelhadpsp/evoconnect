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
    public class PaymentDA(ClinicDbContext context) : IPaymentDA
    {
        private readonly ClinicDbContext _context = context;

        public async Task<PaginatedResult<PaymentDto>> GetPaginatedPaymentsAsync(PaymentFilterRequest request)
        {
            try
            {
                var validatedRequest = ValidateRequest(request);

                // Base query avec les filtres
                var query = BuildBaseQuery(validatedRequest.Filters);

                // Get total count
                int totalCount = await query.CountAsync();

                // Apply pagination and sorting
                var pagedQuery = ApplyPaginationAndSorting(query, validatedRequest);

                // Execute query and map to DTOs
                var planAppliques = await pagedQuery.ToListAsync();
                var payments = planAppliques.Select(MapToPaymentDto).ToList();

                // Create pagination metadata
                var metadata = CreatePaginationMetadata(validatedRequest.Pagination, totalCount);

                return new PaginatedResult<PaymentDto>
                {
                    Items = payments,
                    Metadata = metadata
                };
            }
            catch (Exception)
            {
                return new PaginatedResult<PaymentDto>
                {
                    Items = new List<PaymentDto>(),
                    Metadata = new PaginationMetadata()
                };
            }
        }

        public async Task<int> GetPaymentCountAsync(PaymentFilters filters)
        {
            try
            {
                var query = BuildBaseQuery(filters);
                return await query.CountAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<List<PaymentDto>> GetPaymentsByPatientIdAsync(int patientId)
        {
            try
            {
                var planAppliques = await _context.PlansAppliques
                    .Include(pa => pa.Patient)
                        .ThenInclude(p => p.Personne)
                    .Include(pa => pa.Praticien)
                    .Include(pa => pa.Mutuelle)
                    .Include(pa => pa.Caisse)
                    .Where(pa => pa.IdPatient == patientId && pa.TypeRegle == 0)
                    .OrderByDescending(pa => pa.DateRens)
                    .ThenByDescending(pa => pa.DateRefPaie)
                    .ToListAsync();

                return planAppliques.Select(MapToPaymentDto).ToList();
            }
            catch (Exception)
            {
                return new List<PaymentDto>();
            }
        }
        public async Task<float> GetTotalPaymentsToday()
        {

            var Today = DateTime.Today;
            var Tomorrow = Today.AddDays(1);

            var totalPayments = await _context.PlansAppliques
               .Where(pa => pa.DateRens >= Today && pa.DateRens < Tomorrow && pa.TypeRegle == 0)
               .SumAsync(pa => pa.MontantEncais ?? 0);

            return totalPayments;

        }

        public async Task<PaymentDto> GetPaymentByIdAsync(int patientId, int codePlan, int numDate)
        {
            try
            {
                var planApplique = await _context.PlansAppliques
                    .Include(pa => pa.Patient)
                        .ThenInclude(p => p.Personne)
                    .Include(pa => pa.Praticien)
                    .Include(pa => pa.Mutuelle)
                    .Include(pa => pa.Caisse)
                    .FirstOrDefaultAsync(pa =>
                        pa.IdPatient == patientId &&
                        pa.CodePlan == codePlan &&
                        pa.NumDate == numDate);

                return planApplique != null ? MapToPaymentDto(planApplique) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var planAppliques = await _context.PlansAppliques
                    .Include(pa => pa.Patient)
                        .ThenInclude(p => p.Personne)
                    .Where(pa => pa.DateRens >= fromDate &&
                                pa.DateRens <= toDate &&
                                pa.TypeRegle == 0)
                    .OrderByDescending(pa => pa.DateRens)
                    .ToListAsync();

                return planAppliques.Select(MapToPaymentDto).ToList();
            }
            catch (Exception)
            {
                return new List<PaymentDto>();
            }
        }

        public async Task<List<PaymentDto>> GetOutstandingPaymentsAsync()
        {
            try
            {
                var planAppliques = await _context.PlansAppliques
                    .Include(pa => pa.Patient)
                        .ThenInclude(p => p.Personne)
                    .Where(pa => (pa.MontantEncais - (pa.MontantEncais ?? 0)) > 0.01 &&
                                pa.TypeRegle == 0)
                    .OrderBy(pa => pa.DateRefPaie)
                    .ToListAsync();

                return planAppliques.Select(MapToPaymentDto).ToList();
            }
            catch (Exception)
            {
                return new List<PaymentDto>();
            }
        }

        public async Task<PaymentStatistics> GetPaymentStatisticsAsync(int patientId)
        {
            try
            {
                var payments = await _context.PlansAppliques
                    .Where(pa => pa.IdPatient == patientId && pa.TypeRegle == 0)
                    .ToListAsync();

                if (!payments.Any())
                {
                    return new PaymentStatistics { PatientId = patientId };
                }

                return new PaymentStatistics
                {
                    PatientId = patientId,
                    TotalPayments = payments.Count,
                    TotalAmount = payments.Sum(pa => pa.MontantEncais ?? 0),
                    TotalAmountEuro = payments.Sum(pa => pa.MontantEncaisEuro ?? 0),
                    TotalCollected = payments.Sum(pa => pa.MontantEncais ?? 0),
                    TotalCollectedEuro = payments.Sum(pa => pa.MontantEncaisEuro ?? 0),
                    TotalOutstanding = payments.Sum(pa => (pa.MontantEncais ?? 0) - (pa.MontantEncais ?? 0)),
                    TotalOutstandingEuro = payments.Sum(pa => (pa.MontantEncaisEuro ?? 0) - (pa.MontantEncaisEuro ?? 0)),
                    PaidPayments = payments.Count(pa => Math.Abs((pa.MontantEncais ?? 0) - (pa.MontantEncais ?? 0)) < 0.01),
                    PendingPayments = payments.Count(pa => ((pa.MontantEncais ?? 0) - (pa.MontantEncais ?? 0)) > 0.01),
                    OverduePayments = payments.Count(pa => ((pa.MontantEncais ?? 0) - (pa.MontantEncais ?? 0)) > 0.01 &&
                                                          pa.DateRefPaie < DateTime.Now.AddDays(-30)),
                    LastPaymentDate = payments.Max(pa => pa.DateRens),
                    NextPaymentDate = payments.Where(pa => ((pa.MontantEncais ?? 0) - (pa.MontantEncais ?? 0)) > 0.01)
                                             .Min(pa => pa.DateRefPaie),
                    AveragePaymentAmount = payments.Average(pa => pa.MontantEncais ?? 0)
                };
            }
            catch (Exception)
            {
                return new PaymentStatistics { PatientId = patientId };
            }
        }

        public async Task<List<PaymentDto>> GetPaymentsByModeAsync(PaymentMode paymentMode)
        {
            try
            {
                var planAppliques = await _context.PlansAppliques
                    .Include(pa => pa.Patient)
                        .ThenInclude(p => p.Personne)
                    .Where(pa => pa.ModePay == (short)paymentMode && pa.TypeRegle == 0)
                    .OrderByDescending(pa => pa.DateRens)
                    .ToListAsync();

                return planAppliques.Select(MapToPaymentDto).ToList();
            }
            catch (Exception)
            {
                return new List<PaymentDto>();
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int patientId, int codePlan, int numDate, PaymentStatus status)
        {
            try
            {
                var planApplique = await _context.PlansAppliques
                    .FirstOrDefaultAsync(pa =>
                        pa.IdPatient == patientId &&
                        pa.CodePlan == codePlan &&
                        pa.NumDate == numDate);

                if (planApplique == null)
                    return false;

                planApplique.Remarque = status.ToString();

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region Dashboard Methods

        public async Task<PaymentDashboardStats> GetPaymentDashboardStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.PlansAppliques.Where(pa => pa.TypeRegle == 0);

                if (fromDate.HasValue)
                    query = query.Where(pa => pa.DateRens >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pa => pa.DateRens <= toDate.Value);

                var payments = await query.ToListAsync();

                var stats = new PaymentDashboardStats();

                if (payments.Any())
                {
                    stats.TotalPayments = payments.Count;
                    stats.TotalAmount = payments.Sum(pa => pa.MontantEncais ?? 0);
                    stats.TotalAmountEuro = payments.Sum(pa => pa.MontantEncaisEuro ?? 0);
                    stats.AveragePaymentAmount = payments.Average(pa => pa.MontantEncais ?? 0);
                    stats.MaxPaymentAmount = payments.Max(pa => pa.MontantEncais ?? 0);
                    stats.MinPaymentAmount = payments.Min(pa => pa.MontantEncais ?? 0);
                    stats.LastPaymentDate = payments.Max(pa => pa.DateRens);
                    stats.FirstPaymentDate = payments.Min(pa => pa.DateRens);

                    // Payment mode statistics
                    stats.PaymentModeCounts = payments
                        .GroupBy(pa => pa.ModePay ?? 0)
                        .ToDictionary(g => g.Key, g => g.Count());

                    stats.PaymentModeAmounts = payments
                        .GroupBy(pa => pa.ModePay ?? 0)
                        .ToDictionary(g => g.Key, g => g.Sum(pa => pa.MontantEncais ?? 0));
                }

                return stats;
            }
            catch (Exception)
            {
                return new PaymentDashboardStats();
            }
        }

        public async Task<Dictionary<int, float>> GetMonthlyRevenueAsync(int year)
        {
            try
            {
                var monthlyRevenue = await _context.PlansAppliques
                    .Where(pa => pa.TypeRegle == 0 &&
                                pa.DateRens.HasValue &&
                                pa.DateRens.Value.Year == year)
                    .GroupBy(pa => pa.DateRens.Value.Month)
                    .Select(g => new { Month = g.Key, Total = g.Sum(pa => pa.MontantEncais ?? 0) })
                    .ToDictionaryAsync(x => x.Month, x => x.Total);


                var result = new Dictionary<int, float>();
                for (int i = 1; i <= 12; i++)
                {
                    result[i] = monthlyRevenue.ContainsKey(i) ? monthlyRevenue[i] : 0.0f;
                }

                return result;
            }
            catch (Exception)
            {
                return new Dictionary<int, float>();
            }
        }

        public async Task<Dictionary<int, float>> GetWeeklyRevenueAsync(int year)
        {
            try
            {
                var payments = await _context.PlansAppliques
                    .Where(pa => pa.TypeRegle == 0 &&
                                pa.DateRens.HasValue &&
                                pa.DateRens.Value.Year == year)
                    .Select(pa => new { pa.DateRens, pa.MontantEncais })
                    .ToListAsync();

                var weeklyRevenue = payments
                    .GroupBy(pa => GetWeekOfYear(pa.DateRens.Value))
                    .ToDictionary(g => g.Key, g => g.Sum(pa => pa.MontantEncais ?? 0));

                return weeklyRevenue;
            }
            catch (Exception)
            {
                return new Dictionary<int, float>();
            }
        }

        public async Task<Dictionary<int, float>> GetYearlyRevenueAsync(int startYear, int endYear)
        {
            try
            {
                var yearlyRevenue = await _context.PlansAppliques
                    .Where(pa => pa.TypeRegle == 0 &&
                                pa.DateRens.HasValue &&
                                pa.DateRens.Value.Year >= startYear &&
                                pa.DateRens.Value.Year <= endYear)
                    .GroupBy(pa => pa.DateRens.Value.Year)
                    .Select(g => new { Year = g.Key, Total = g.Sum(pa => pa.MontantEncais ?? 0) })
                    .ToDictionaryAsync(x => x.Year, x => x.Total);

                return yearlyRevenue;
            }
            catch (Exception)
            {
                return new Dictionary<int, float>();
            }
        }

        public async Task<List<TopPatientPayment>> GetTopPatientsByPaymentAsync(int limit = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.PlansAppliques
                    .Include(pa => pa.Patient)
                        .ThenInclude(p => p.Personne)
                    .Where(pa => pa.TypeRegle == 0);

                if (fromDate.HasValue)
                    query = query.Where(pa => pa.DateRens >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pa => pa.DateRens <= toDate.Value);

                var topPatients = await query
                    .GroupBy(pa => new
                    {
                        pa.IdPatient,
                        pa.Patient.Personne.PerNom,
                        pa.Patient.Personne.PerPrenom,
                        pa.Patient.PatNumDossier
                    })
                    .Select(g => new TopPatientPayment
                    {
                        PatientId = g.Key.IdPatient,
                        PatientName = g.Key.PerNom ?? "",
                        PatientFirstName = g.Key.PerPrenom ?? "",
                        DossierNumber = g.Key.PatNumDossier,
                        PaymentCount = g.Count(),
                        TotalAmount = g.Sum(pa => pa.MontantEncais ?? 0),
                        AverageAmount = g.Average(pa => pa.MontantEncais ?? 0),
                        LastPaymentDate = g.Max(pa => pa.DateRens)
                    })
                    .OrderByDescending(tp => tp.TotalAmount)
                    .Take(limit)
                    .ToListAsync();

                return topPatients;
            }
            catch (Exception)
            {
                return new List<TopPatientPayment>();
            }
        }

        #endregion

        #region Helper Methods

        private IQueryable<PlanApplique> BuildBaseQuery(PaymentFilters filters)
        {
            var query = _context.PlansAppliques
                .Include(pa => pa.Patient)
                    .ThenInclude(p => p.Personne)
                .Include(pa => pa.Praticien)
                .Include(pa => pa.Mutuelle)
                .Include(pa => pa.Caisse)
                .Where(pa => pa.TypeRegle == 0);

            // Apply filters
            if (filters.PatientId.HasValue)
                query = query.Where(pa => pa.IdPatient == filters.PatientId.Value);

            if (!string.IsNullOrWhiteSpace(filters.PatientName))
            {
                var name = filters.PatientName.ToLower();
                query = query.Where(pa =>
                    (pa.Patient.Personne.PerNom != null && pa.Patient.Personne.PerNom.ToLower().Contains(name)) ||
                    (pa.Patient.Personne.PerPrenom != null && pa.Patient.Personne.PerPrenom.ToLower().Contains(name)) ||
                    (pa.Patient.Personne.PerNom != null && pa.Patient.Personne.PerPrenom != null &&
                     (pa.Patient.Personne.PerNom + " " + pa.Patient.Personne.PerPrenom).ToLower().Contains(name)));
            }

            if (filters.DossierNumber.HasValue)
                query = query.Where(pa => pa.Patient.PatNumDossier == filters.DossierNumber.Value);

            if (filters.DateFrom.HasValue)
                query = query.Where(pa => pa.DateRens >= filters.DateFrom.Value);

            if (filters.DateTo.HasValue)
                query = query.Where(pa => pa.DateRens <= filters.DateTo.Value);

            if (filters.PaymentRefDateFrom.HasValue)
                query = query.Where(pa => pa.DateRefPaie >= filters.PaymentRefDateFrom.Value);

            if (filters.PaymentRefDateTo.HasValue)
                query = query.Where(pa => pa.DateRefPaie <= filters.PaymentRefDateTo.Value);

            if (filters.AmountFrom.HasValue)
                query = query.Where(pa => pa.MontantEncais >= filters.AmountFrom.Value);

            if (filters.AmountTo.HasValue)
                query = query.Where(pa => pa.MontantEncais <= filters.AmountTo.Value);

            if (filters.PaymentMode.HasValue)
                query = query.Where(pa => pa.ModePay == (short)filters.PaymentMode.Value);

            if (!string.IsNullOrWhiteSpace(filters.PaymentLabel))
                query = query.Where(pa => pa.Libelle != null && pa.Libelle.ToLower().Contains(filters.PaymentLabel.ToLower()));

            if (filters.PlanCode.HasValue)
                query = query.Where(pa => pa.CodePlan == filters.PlanCode.Value);

            if (filters.PlanId.HasValue)
                query = query.Where(pa => pa.IdPlan == filters.PlanId.Value);

            if (filters.PractitionerId.HasValue)
                query = query.Where(pa => pa.IdPraticien == filters.PractitionerId.Value);

            if (filters.HasALD.HasValue)
                query = query.Where(pa => filters.HasALD.Value ? pa.Ald == 1 : (pa.Ald == 0 || pa.Ald == null));

            if (filters.MutuelleId.HasValue)
                query = query.Where(pa => pa.IdMutuelle == filters.MutuelleId.Value);

            if (filters.CaisseId.HasValue)
                query = query.Where(pa => pa.IdCaisse == filters.CaisseId.Value);

            return query;
        }

        private IQueryable<PlanApplique> ApplyPaginationAndSorting(IQueryable<PlanApplique> query, PaymentFilterRequest request)
        {
            // Apply sorting
            query = request.Sorting.SortBy switch
            {
                PaymentSortFields.Date => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(pa => pa.DateRens)
                    : query.OrderBy(pa => pa.DateRens),
                PaymentSortFields.PaymentRefDate => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(pa => pa.DateRefPaie)
                    : query.OrderBy(pa => pa.DateRefPaie),
                PaymentSortFields.Amount => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(pa => pa.Montant)
                    : query.OrderBy(pa => pa.Montant),
                PaymentSortFields.CollectedAmount => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(pa => pa.MontantEncais)
                    : query.OrderBy(pa => pa.MontantEncais),
                PaymentSortFields.PatientName => request.Sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(pa => pa.Patient.Personne.PerNom)
                    : query.OrderBy(pa => pa.Patient.Personne.PerNom),
                _ => query.OrderByDescending(pa => pa.DateRens)
            };

            // Apply pagination
            if (request.Pagination.ValidatedPageSize > 0 && request.Pagination.ValidatedPageNumber > 0)
            {
                var skip = (request.Pagination.ValidatedPageNumber - 1) * request.Pagination.ValidatedPageSize;
                query = query.Skip(skip).Take(request.Pagination.ValidatedPageSize);
            }

            return query;
        }

        private PaymentDto MapToPaymentDto(PlanApplique planApplique)
        {
            return new PaymentDto
            {
                PatientId = planApplique.IdPatient,
                PlanCode = planApplique.CodePlan,
                NumDate = planApplique.NumDate,
                Date = planApplique.DateRens,
                PaymentRefDate = planApplique.DateRefPaie,
                PaymentType = planApplique.TypeRegle,
                PaymentLabel = planApplique.Libelle?.Trim() ?? "",
                Amount = planApplique.Montant,
                AmountEuro = planApplique.MontantEuro,
                CollectedAmount = planApplique.MontantEncais,
                CollectedAmountEuro = planApplique.MontantEncaisEuro,
                PaymentModeValue = planApplique.ModePay.HasValue ? (PaymentMode)planApplique.ModePay.Value : null,
                BankFlag = planApplique.FlagBanque?.Trim() ?? "",
                BankName = planApplique.Banque?.Trim() ?? "",
                CheckNumber = planApplique.NumCheque?.Trim() ?? "",
                BankDepositDate = planApplique.DateRemBanque,
                PlanId = planApplique.IdPlan,
                Tariff = planApplique.Tarif?.Trim() ?? "",
                DateRef = planApplique.DateRef,
                NbDays = planApplique.NbJours,
                NbMonths = planApplique.NbMois,
                HasALD = planApplique.Ald == 1,
                HasExoneration = planApplique.Exoneration == 1,
                IsAccident = planApplique.Accident == 1,
                IsSundayHoliday = planApplique.DimancheFerie == 1,
                IsNight = planApplique.Nuit == 1,
                ToothNumber = planApplique.NumDent,
                TeethList = planApplique.Dents?.Trim() ?? "",
                AccidentDate = planApplique.DateAccident,
                ThirdPartyAmount = planApplique.MontantTiersPayant,
                MutuelleAmount = planApplique.MontantMutuelle,
                MutuelleId = planApplique.IdMutuelle,
                CaisseId = planApplique.IdCaisse,
                PatientName = planApplique.Patient?.Personne?.PerNom?.Trim() ?? "",
                PatientFirstName = planApplique.Patient?.Personne?.PerPrenom?.Trim() ?? "",
                DossierNumber = planApplique.Patient?.PatNumDossier ?? 0
            };
        }

        private PaymentFilterRequest ValidateRequest(PaymentFilterRequest request)
        {
            if (request == null)
                request = new PaymentFilterRequest();

            request.Filters ??= new PaymentFilters();
            request.Pagination ??= new PaginationRequest();
            request.Sorting ??= new PaymentSortingRequest();

            return request;
        }

        private PaginationMetadata CreatePaginationMetadata(PaginationRequest pagination, int totalItems)
        {
            var totalPages = (int)Math.Ceiling((float)totalItems / pagination.ValidatedPageSize);

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

        private int GetWeekOfYear(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            return calendar.GetWeekOfYear(date, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
        }

        #endregion

        #region Additional Helper Methods

        public async Task<PaymentSummary> GetPaymentsSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.PlansAppliques.Where(pa => pa.TypeRegle == 0);

                if (fromDate.HasValue)
                    query = query.Where(pa => pa.DateRens >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pa => pa.DateRens <= toDate.Value);

                var payments = await query.ToListAsync();

                if (!payments.Any())
                    return new PaymentSummary();

                return new PaymentSummary
                {
                    TotalPayments = payments.Count,
                    TotalAmount = payments.Sum(pa => pa.MontantEncais ?? 0),
                    TotalAmountEuro = payments.Sum(pa => pa.MontantEncaisEuro ?? 0),
                    TotalCollected = payments.Sum(pa => pa.MontantEncais ?? 0),
                    TotalCollectedEuro = payments.Sum(pa => pa.MontantEncaisEuro ?? 0),
                    TotalOutstanding = payments.Sum(pa => (pa.MontantEncais ?? 0) - (pa.MontantEncais ?? 0)),
                    OutstandingCount = payments.Count(pa => ((pa.MontantEncais ?? 0) - (pa.MontantEncais ?? 0)) > 0.01),
                    PaidCount = payments.Count(pa => Math.Abs((pa.MontantEncais ?? 0) - (pa.MontantEncais ?? 0)) < 0.01),
                    AveragePaymentAmount = payments.Average(pa => pa.MontantEncais ?? 0)
                };
            }
            catch (Exception)
            {
                return new PaymentSummary();
            }
        }

        public async Task<Dictionary<DateTime, float>> GetDailyRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                
                var dailyRevenue = await _context.PlansAppliques
                    .Where(pa => pa.TypeRegle == 0 &&
                                pa.DateRens.HasValue &&
                                pa.DateRens.Value.Date >= fromDate.Date &&
                                pa.DateRens.Value.Date <= toDate.Date)
                    .GroupBy(pa => pa.DateRens.Value.Date)
                    .Select(g => new { Date = g.Key, Total = g.Sum(pa => pa.MontantEncais ?? 0) })
                    .ToDictionaryAsync(x => x.Date, x => x.Total);

                // Fill in missing days with 0
                var result = new Dictionary<DateTime, float>();
                for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
                {
                    result[date] = dailyRevenue.ContainsKey(date) ? dailyRevenue[date] : 0.0f;
                }

                return result;
            }
            catch (Exception)
            {
                return new Dictionary<DateTime, float>();
            }
        }

        public async Task<List<PaymentDto>> GetOverduePaymentsAsync(int daysPastDue = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysPastDue);
                var planAppliques = await _context.PlansAppliques
                    .Include(pa => pa.Patient)
                        .ThenInclude(p => p.Personne)
                    .Where(pa => (pa.MontantEncais - (pa.MontantEncais ?? 0)) > 0.01 &&
                                pa.TypeRegle == 0 &&
                                pa.DateRefPaie < cutoffDate)
                    .OrderBy(pa => pa.DateRefPaie)
                    .ToListAsync();

                return planAppliques.Select(MapToPaymentDto).ToList();
            }
            catch (Exception)
            {
                return new List<PaymentDto>();
            }
        }

        public async Task<List<PaymentDto>> GetPaymentsByBankDepositAsync(DateTime depositDate)
        {
            try
            {
                var planAppliques = await _context.PlansAppliques
                    .Include(pa => pa.Patient)
                        .ThenInclude(p => p.Personne)
                    .Where(pa => pa.DateRemBanque.HasValue &&
                                pa.DateRemBanque.Value.Date == depositDate.Date &&
                                pa.TypeRegle == 0)
                    .OrderByDescending(pa => pa.MontantEncais)
                    .ToListAsync();

                return planAppliques.Select(MapToPaymentDto).ToList();
            }
            catch (Exception)
            {
                return new List<PaymentDto>();
            }
        }

        #endregion
        #region Date Gap Filling Helper Methods

        /// <summary>
        /// Fills missing days in a date range with zero values
        /// </summary>
        private Dictionary<DateTime, float> FillDailyGaps(Dictionary<DateTime, float> data, DateTime fromDate, DateTime toDate)
        {
            var result = new Dictionary<DateTime, float>();

            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                result[date] = data.ContainsKey(date) ? data[date] : 0.0f;
            }

            return result;
        }

        /// <summary>
        /// Fills missing months in a year with zero values
        /// </summary>
        private Dictionary<int, float> FillMonthlyGaps(Dictionary<int, float> data, int startMonth = 1, int endMonth = 12)
        {
            var result = new Dictionary<int, float>();

            for (int month = startMonth; month <= endMonth; month++)
            {
                result[month] = data.ContainsKey(month) ? data[month] : 0.0f;
            }

            return result;
        }

        /// <summary>
        /// Fills missing weeks in a year with zero values (1-53)
        /// </summary>
        private Dictionary<int, float> FillWeeklyGaps(Dictionary<int, float> data, int maxWeeks = 53)
        {
            var result = new Dictionary<int, float>();

            for (int week = 1; week <= maxWeeks; week++)
            {
                result[week] = data.ContainsKey(week) ? data[week] : 0.0f;
            }

            return result;
        }

        /// <summary>
        /// Fills missing years in a range with zero values
        /// </summary>
        private Dictionary<int, float> FillYearlyGaps(Dictionary<int, float> data, int startYear, int endYear)
        {
            var result = new Dictionary<int, float>();

            for (int year = startYear; year <= endYear; year++)
            {
                result[year] = data.ContainsKey(year) ? data[year] : 0.0f;
            }

            return result;
        }

        #endregion
    }
}