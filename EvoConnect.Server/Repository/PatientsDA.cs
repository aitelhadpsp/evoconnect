using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Server.Models;
using EvoConnect.Server.Data;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using EvoConnect.Server.DTOs;

namespace EvoConnect.Server.Repository
{
    public class PatientsDA(ClinicDbContext _context) : IPatientsDA
    {
        public async Task<PaginatedResult<EvocomPatientDto>> GetPaginatedPatientsAsync(PatientFilterRequest request)
        {
            try
            {
                var validatedRequest = ValidateRequest(request);

                // Build base query
                var query = BuildPatientQuery(_context.Patients.WithPersonne());

                // Apply filters
                query = ApplyFilters(query, validatedRequest.Filters);

                // Get total count
                int totalCount = await query.CountAsync();

                // Apply sorting and pagination
                query = ApplySorting(query, validatedRequest.Sorting);
                query = ApplyPagination(query, validatedRequest.Pagination);

                // Execute query and map to DTOs
                var patients = await query.ToListAsync();
                var patientDtos = patients.Select(MapToPatientDto).ToList();

                // Create pagination metadata
                var metadata = CreatePaginationMetadata(validatedRequest.Pagination, totalCount);

                return new PaginatedResult<EvocomPatientDto>
                {
                    Items = patientDtos,
                    Metadata = metadata
                };
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return new PaginatedResult<EvocomPatientDto>
                {
                    Items = new List<EvocomPatientDto>(),
                    Metadata = new PaginationMetadata()
                };
            }
        }

        public async Task<int> GetPatientCountAsync(PatientFilters filters)
        {
            try
            {
                var query = BuildPatientQuery(_context.Patients.WithPersonne());
                query = ApplyFilters(query, filters);
                return await query.CountAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<EvocomPatientDto> GetPatientByIdAsync(int patientId)
        {
            try
            {
                var patient = await _context.Patients
                    .WithPersonne()
                    .FirstOrDefaultAsync(p => p.IdPersonne == patientId);

                return patient != null ? MapToPatientDto(patient) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PaginatedResult<EvocomPatientDto>> SearchPatientsAsync(string searchText, int pageNumber = 1, int pageSize = 20)
        {
            var filters = new PatientFilters();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filters.Name = searchText;
            }

            var request = new PatientFilterRequest
            {
                Filters = filters,
                Pagination = new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize },
                Sorting = new SortingRequest { SortBy = PatientSortFields.LastName, Direction = SortDirection.Ascending }
            };

            return await GetPaginatedPatientsAsync(request);
        }

        #region Private Methods

        private IQueryable<Patient> BuildPatientQuery(IQueryable<Patient> baseQuery)
        {
            return baseQuery.Where(p => p.Personne.IdPersonne > 0);
        }

        private IQueryable<Patient> ApplyFilters(IQueryable<Patient> query, PatientFilters filters)
        {
            if (filters == null) return query;

            // Name search (handles both individual name and combined search)
            if (!string.IsNullOrWhiteSpace(filters.Name))
            {
                var searchTerm = filters.Name.Trim().ToUpper();
                query = query.Where(p =>
                    p.Personne.PerNom.ToUpper().Contains(searchTerm) ||
                    p.Personne.PerPrenom.ToUpper().Contains(searchTerm) ||
                    (p.Personne.PerNom + " " + p.Personne.PerPrenom).ToUpper().Contains(searchTerm) ||
                    p.PatNumDossier.ToString().Contains(searchTerm) ||
                    (p.PatRefDossier != null && p.PatRefDossier.ToUpper().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(filters.FirstName))
            {
                var firstName = filters.FirstName.Trim().ToUpper();
                query = query.Where(p => p.Personne.PerPrenom.ToUpper().Contains(firstName));
            }

            if (!string.IsNullOrWhiteSpace(filters.Email))
            {
                var email = filters.Email.Trim().ToUpper();
                query = query.Where(p =>
                    (p.Personne.PerEmail != null && p.Personne.PerEmail.ToUpper().Contains(email)) ||
                    (p.Personne.Email2 != null && p.Personne.Email2.ToUpper().Contains(email)) ||
                    (p.Personne.Email3 != null && p.Personne.Email3.ToUpper().Contains(email))
                );
            }

            if (!string.IsNullOrWhiteSpace(filters.Phone))
            {
                var phone = filters.Phone.Trim();
                query = query.Where(p =>
                    (p.Personne.PerTelPrinc != null && p.Personne.PerTelPrinc.Contains(phone)) ||
                    (p.Personne.PerTelTrav1 != null && p.Personne.PerTelTrav1.Contains(phone)) ||
                    (p.Personne.PerTelTrav2 != null && p.Personne.PerTelTrav2.Contains(phone)) ||
                    (p.Personne.Gsm != null && p.Personne.Gsm.Contains(phone))
                );
            }

            if (!string.IsNullOrWhiteSpace(filters.SecuriteSociale))
            {
                var secu = filters.SecuriteSociale.Trim();
                query = query.Where(p => p.Personne.PerSecu != null && p.Personne.PerSecu.Contains(secu));
            }

            if (filters.Gender.HasValue)
            {
                query = query.Where(p => p.Personne.PerGenre == filters.Gender.Value.ToString());
            }

            if (filters.BirthDateFrom.HasValue)
            {
                query = query.Where(p => p.Personne.PerDatNaiss >= filters.BirthDateFrom.Value);
            }

            if (filters.BirthDateTo.HasValue)
            {
                query = query.Where(p => p.Personne.PerDatNaiss <= filters.BirthDateTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.City))
            {
                var city = filters.City.Trim().ToUpper();
                query = query.Where(p => p.Personne.PerVille != null && p.Personne.PerVille.ToUpper().Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(filters.PostalCode))
            {
                var postalCode = filters.PostalCode.Trim();
                query = query.Where(p => p.Personne.PerCPostal != null && p.Personne.PerCPostal.Contains(postalCode));
            }

            // Patient-specific filters
            if (filters.DossierNumber.HasValue)
            {
                query = query.Where(p => p.PatNumDossier == filters.DossierNumber.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.DossierReference))
            {
                var dossierRef = filters.DossierReference.Trim().ToUpper();
                query = query.Where(p => p.PatRefDossier != null && p.PatRefDossier.ToUpper().Contains(dossierRef));
            }

            if (filters.CreationDateFrom.HasValue)
            {
                var fromDateOnly = DateOnly.FromDateTime(filters.CreationDateFrom.Value);
                query = query.Where(p => p.PatDateCreation >= fromDateOnly);
            }

            if (filters.CreationDateTo.HasValue)
            {
                var toDateOnly = DateOnly.FromDateTime(filters.CreationDateTo.Value);
                query = query.Where(p => p.PatDateCreation <= toDateOnly);
            }

            if (filters.LastAppointmentFrom.HasValue)
            {
                query = query.Where(p => p.PatLastRdv >= filters.LastAppointmentFrom.Value);
            }

            if (filters.LastAppointmentTo.HasValue)
            {
                query = query.Where(p => p.PatLastRdv <= filters.LastAppointmentTo.Value);
            }

            if (filters.NextAppointmentFrom.HasValue)
            {
                query = query.Where(p => p.PatDateRdv >= filters.NextAppointmentFrom.Value);
            }

            if (filters.NextAppointmentTo.HasValue)
            {
                query = query.Where(p => p.PatDateRdv <= filters.NextAppointmentTo.Value);
            }

            // Financial filters
            if (filters.BalanceFrom.HasValue)
            {
                query = query.Where(p => p.PatSolde >= filters.BalanceFrom.Value);
            }

            if (filters.BalanceTo.HasValue)
            {
                query = query.Where(p => p.PatSolde <= filters.BalanceTo.Value);
            }

            if (filters.HasPositiveBalance == true)
            {
                query = query.Where(p => p.PatSolde > 0);
            }

            if (filters.HasNegativeBalance == true)
            {
                query = query.Where(p => p.PatSolde < 0);
            }

            // Status filters
            if (filters.StatusId.HasValue)
            {
                query = query.Where(p => p.IdStatut == filters.StatusId.Value);
            }

            if (filters.HasCMU.HasValue)
            {
                if (filters.HasCMU.Value)
                    query = query.Where(p => p.Cmu == 1);
                else
                    query = query.Where(p => p.Cmu == null || p.Cmu == 0);
            }

            if (filters.HasTiersPayant.HasValue)
            {
                if (filters.HasTiersPayant.Value)
                    query = query.Where(p => p.TiersPayant == 1);
                else
                    query = query.Where(p => p.TiersPayant == null || p.TiersPayant == 0);
            }

            if (filters.HasAllergies.HasValue)
            {
                if (filters.HasAllergies.Value)
                    query = query.Where(p => p.Allergie != null);
                else
                    query = query.Where(p => p.Allergie == null);
            }

            if (filters.HasALD.HasValue)
            {
                if (filters.HasALD.Value)
                    query = query.Where(p => p.Ald == 1);
                else
                    query = query.Where(p => p.Ald == null || p.Ald == 0);
            }

            if (!string.IsNullOrWhiteSpace(filters.TreatmentPhase))
            {
                var treatmentPhase = filters.TreatmentPhase.Trim().ToUpper();
                query = query.Where(p => p.PhaseTrait != null && p.PhaseTrait.ToUpper().Contains(treatmentPhase));
            }

            if (filters.TreatmentStartFrom.HasValue)
            {
                query = query.Where(p => p.DebutTrait >= filters.TreatmentStartFrom.Value);
            }

            if (filters.TreatmentStartTo.HasValue)
            {
                query = query.Where(p => p.DebutTrait <= filters.TreatmentStartTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.DossierType))
            {
                var dossierType = filters.DossierType.Trim().ToUpper();
                query = query.Where(p => p.TyDossier != null && p.TyDossier.ToUpper().Contains(dossierType));
            }

            if (filters.EmailAuthorized.HasValue)
            {
                if (filters.EmailAuthorized.Value)
                    query = query.Where(p => p.AuthMail == 1);
                else
                    query = query.Where(p => p.AuthMail == null || p.AuthMail == 0);
            }

            if (filters.LastModifiedFrom.HasValue)
            {
                query = query.Where(p => p.Personne.LastModif >= filters.LastModifiedFrom.Value);
            }

            if (filters.LastModifiedTo.HasValue)
            {
                query = query.Where(p => p.Personne.LastModif <= filters.LastModifiedTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.Profession))
            {
                var profession = filters.Profession.Trim().ToUpper();
                query = query.Where(p => p.Personne.Profession != null && p.Personne.Profession.ToUpper().Contains(profession));
            }

            if (!string.IsNullOrWhiteSpace(filters.Mutuelle))
            {
                var mutuelle = filters.Mutuelle.Trim().ToUpper();
                query = query.Where(p => p.Personne.Mutuelle != null && p.Personne.Mutuelle.ToUpper().Contains(mutuelle));
            }

            if (filters.ExcludePatientIds?.Any() == true)
            {
                query = query.Where(p => !filters.ExcludePatientIds.Contains(p.Personne.IdPersonne));
            }

            return query;
        }

        private IQueryable<Patient> ApplySorting(IQueryable<Patient> query, SortingRequest sorting)
        {
            if (sorting == null)
                return query.OrderBy(p => p.Personne.PerNom);

            return sorting.SortBy switch
            {
                PatientSortFields.LastName => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.Personne.PerNom)
                    : query.OrderBy(p => p.Personne.PerNom),

                PatientSortFields.FirstName => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.Personne.PerPrenom)
                    : query.OrderBy(p => p.Personne.PerPrenom),

                PatientSortFields.DossierNumber => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.PatNumDossier)
                    : query.OrderBy(p => p.PatNumDossier),

                PatientSortFields.CreationDate => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.PatDateCreation)
                    : query.OrderBy(p => p.PatDateCreation),

                PatientSortFields.LastAppointment => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.PatLastRdv)
                    : query.OrderBy(p => p.PatLastRdv),

                PatientSortFields.NextAppointment => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.PatDateRdv)
                    : query.OrderBy(p => p.PatDateRdv),

                PatientSortFields.Balance => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.PatSolde)
                    : query.OrderBy(p => p.PatSolde),

                PatientSortFields.LastModified => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.Personne.LastModif)
                    : query.OrderBy(p => p.Personne.LastModif),

                PatientSortFields.BirthDate => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.Personne.PerDatNaiss)
                    : query.OrderBy(p => p.Personne.PerDatNaiss),

                PatientSortFields.City => sorting.Direction == SortDirection.Descending
                    ? query.OrderByDescending(p => p.Personne.PerVille)
                    : query.OrderBy(p => p.Personne.PerVille),

                _ => query.OrderBy(p => p.Personne.PerNom)
            };
        }

        private IQueryable<Patient> ApplyPagination(IQueryable<Patient> query, PaginationRequest pagination)
        {
            if (pagination == null) return query;

            return query
                .Skip((pagination.ValidatedPageNumber - 1) * pagination.ValidatedPageSize)
                .Take(pagination.ValidatedPageSize);
        }

        private EvocomPatientDto MapToPatientDto(Patient patient)
        {
            var personne = patient.Personne;

            return new EvocomPatientDto
            {
                PersonId = personne.IdPersonne,
                LastName = personne.PerNom?.Trim() ?? "",
                MaidenName = personne.PerNomJf?.Trim() ?? "",
                FirstName = personne.PerPrenom?.Trim() ?? "",
                Gender = personne.PerGenre?.Trim().FirstOrDefault(),
                SecuriteSociale = personne.PerSecu?.Trim() ?? "",
                BirthDate = personne.PerDatNaiss,
                Email = personne.PerEmail?.Trim() ?? "",
                Email2 = personne.Email2?.Trim() ?? "",
                Email3 = personne.Email3?.Trim() ?? "",
                PrimaryPhone = personne.PerTelPrinc?.Trim() ?? "",
                WorkPhone1 = personne.PerTelTrav1?.Trim() ?? "",
                WorkPhone2 = personne.PerTelTrav2?.Trim() ?? "",
                Mobile = personne.Gsm?.Trim() ?? "",
                Fax = personne.PerTelecopie?.Trim() ?? "",
                Address1 = personne.PerAdr1?.Trim() ?? "",
                Address2 = personne.PerAdr2?.Trim() ?? "",
                City = personne.PerVille?.Trim() ?? "",
                PostalCode = personne.PerCPostal?.Trim() ?? "",
                Profession = personne.Profession?.Trim() ?? "",
                Mutuelle = personne.Mutuelle?.Trim() ?? "",
                Title = personne.PersTitre?.Trim() ?? "",
                Website = personne.PersSiteWeb?.Trim() ?? "",
                LastModified = personne.LastModif,
                Country = personne.PaysDom?.Trim() ?? "",
                DossierNumber = patient.PatNumDossier,
                DossierReference = patient.PatRefDossier?.Trim() ?? "",
                CreationDate = patient.PatDateCreation?.ToDateTime(TimeOnly.MinValue),
                NextAppointment = patient.PatDateRdv,
                LastAppointment = patient.PatLastRdv,
                Balance = patient.PatSolde,
                BalanceEuro = (decimal?)patient.PatSoldeEuro,
                TreatmentPhase = patient.PhaseTrait?.Trim() ?? "",
                TreatmentStart = patient.DebutTrait,
                DossierType = patient.TyDossier?.Trim() ?? "",
                StatusId = patient.IdStatut ?? 0,
                HasCMU = patient.Cmu == 1,
                HasTiersPayant = patient.TiersPayant == 1,
                HasALD = patient.Ald == 1,
                EmailAuthorized = patient.AuthMail == 1,
                NextPayment = patient.NextPaiement?.Trim() ?? "",
                NextPaymentDate = DateTime.TryParse(patient.DateNextPaiement ?? "", out var date) ? date : null,
                HospitalEntryDate = patient.DateHospitalEntree,
                HospitalExitDate = patient.DateHospitalSortie,
                HasAllergies = patient.Allergie != null
            };
        }

        private PatientFilterRequest ValidateRequest(PatientFilterRequest request)
        {
            if (request == null)
            {
                request = new PatientFilterRequest();
            }

            request.Filters ??= new PatientFilters();
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

        #region Additional Helper Methods

        /// <summary>
        /// Get patients with upcoming appointments
        /// </summary>
        public async Task<List<EvocomPatientDto>> GetPatientsWithUpcomingAppointmentsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var patients = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.PatDateRdv >= fromDate &&
                               p.PatDateRdv <= toDate &&
                               p.PatDateRdv != null)
                    .OrderBy(p => p.PatDateRdv)
                    .ToListAsync();

                return patients.Select(MapToPatientDto).ToList();
            }
            catch (Exception)
            {
                return new List<EvocomPatientDto>();
            }
        }

        /// <summary>
        /// Get patients with outstanding balance
        /// </summary>
        public async Task<List<EvocomPatientDto>> GetPatientsWithBalanceAsync(bool positiveBalance = true)
        {
            try
            {
                var query = _context.Patients.WithPersonne();

                if (positiveBalance)
                    query = query.Where(p => p.PatSolde > 0);
                else
                    query = query.Where(p => p.PatSolde < 0);

                var patients = await query
                    .OrderByDescending(p => p.PatSolde)
                    .Take(100)
                    .ToListAsync();

                return patients.Select(MapToPatientDto).ToList();
            }
            catch (Exception)
            {
                return new List<EvocomPatientDto>();
            }
        }

        /// <summary>
        /// Get recently created patients
        /// </summary>
        public async Task<List<EvocomPatientDto>> GetRecentPatientsAsync(int days = 30, int limit = 50)
        {
            try
            {
                var cutoffDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-days));

                var patients = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.PatDateCreation >= cutoffDate)
                    .OrderByDescending(p => p.PatDateCreation)
                    .Take(limit)
                    .ToListAsync();

                return patients.Select(MapToPatientDto).ToList();
            }
            catch (Exception)
            {
                return new List<EvocomPatientDto>();
            }
        }

        /// <summary>
        /// Get patient statistics
        /// </summary>
        public async Task<PatientStatistics> GetPatientStatisticsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var thirtyDaysAgo = DateOnly.FromDateTime(now.AddDays(-30));

                var patients = _context.Patients.WithPersonne();

                var stats = new PatientStatistics
                {
                    TotalPatients = await patients.CountAsync(),
                    WithUpcomingAppointments = await patients.CountAsync(p => p.PatDateRdv > now),
                    WithPositiveBalance = await patients.CountAsync(p => p.PatSolde > 0),
                    WithNegativeBalance = await patients.CountAsync(p => p.PatSolde < 0),
                    WithCMU = await patients.CountAsync(p => p.Cmu == 1),
                    WithALD = await patients.CountAsync(p => p.Ald == 1),
                    CreatedLast30Days = await patients.CountAsync(p => p.PatDateCreation >= thirtyDaysAgo),
                    AverageBalance = await patients.AverageAsync(p => p.PatSolde ?? 0),
                    LastPatientCreated = await patients
                        .Where(p => p.PatDateCreation != null)
                        .MaxAsync(p => p.PatDateCreation!.Value.ToDateTime(TimeOnly.MinValue))
                };

                return stats;
            }
            catch (Exception)
            {
                return new PatientStatistics();
            }
        }

        /// <summary>
        /// Update patient last appointment date
        /// </summary>
        public async Task<bool> UpdatePatientLastAppointmentAsync(int patientId, DateTime appointmentDate)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null) return false;

                patient.PatLastRdv = appointmentDate;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Update patient balance
        /// </summary>
        public async Task<bool> UpdatePatientBalanceAsync(int patientId, decimal balance, decimal? balanceEuro = null)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null) return false;

                patient.PatSolde = balance;
                if (balanceEuro.HasValue)
                    patient.PatSoldeEuro = (float)balanceEuro.Value;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get patients by status
        /// </summary>
        public async Task<List<EvocomPatientDto>> GetPatientsByStatusAsync(int statusId, int pageSize = 50)
        {
            try
            {
                var patients = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.IdStatut == statusId)
                    .OrderBy(p => p.Personne.PerNom)
                    .Take(pageSize)
                    .ToListAsync();

                return patients.Select(MapToPatientDto).ToList();
            }
            catch (Exception)
            {
                return new List<EvocomPatientDto>();
            }
        }

        /// <summary>
        /// Get patient creation statistics with automatic grouping by day/month/year based on date range
        /// </summary>
        public async Task<List<PatientCreationStatistic>> GetPatientCreationStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Convert DateTime to DateOnly for comparison
                var fromDateOnly = DateOnly.FromDateTime(fromDate);
                var toDateOnly = DateOnly.FromDateTime(toDate);

                // Determine the grouping level based on date range
                var dateSpan = toDate - fromDate;
                var groupingLevel = DetermineGroupingLevel(dateSpan);

                // Get data from database using EF Core
                var dbResults = await GetPatientCreationDataFromDbAsync(fromDateOnly, toDateOnly, groupingLevel);

                // Fill gaps and return complete dataset
                return FillPatientCreationGaps(dbResults, fromDate, toDate, groupingLevel);
            }
            catch (Exception)
            {
                return new List<PatientCreationStatistic>();
            }
        }

        public async Task<PagedResponse<VipPatientDto>> GetVipPatientsPaginated(VipPatientFilterParams filterParams)
        {
            try
            {
                // Get configuration values
                var vipLastVisitMonths = await _context.KpiConfigs
                    .Where(e => e.KpiCode == "VIP_LAST_VISIT_MONTHS")
                    .Select(e => (int)(e.TargetValue ?? 12))
                    .FirstOrDefaultAsync();

                var vipMinTotalRevenue = await _context.KpiConfigs
                    .Where(e => e.KpiCode == "VIP_TOTAL_REVENUE")
                    .Select(e => (decimal)(e.TargetValue ?? 5000))
                    .FirstOrDefaultAsync();

                var vipMinAnnualRevenue = await _context.KpiConfigs
                    .Where(e => e.KpiCode == "VIP_ANNUAL_REVENUE")
                    .Select(e => (decimal)(e.TargetValue ?? 2000))
                    .FirstOrDefaultAsync();

                var cutoffDate = DateTime.Now.AddMonths(-vipLastVisitMonths);
                var oneYearAgo = DateTime.Now.AddYears(-1);

                var query = from patient in _context.Patients
                            .Include(p => p.Personne)

                            let payments = _context.PlansAppliques
                                .Where(pa => pa.IdPatient == patient.IdPersonne &&
                                            pa.MontantEncais.HasValue)

                            let totalRevenue = payments.Sum(pa => (decimal?)pa.MontantEncais) ?? 0

                            let annualRevenue = payments
                                .Where(pa => pa.DateRens.HasValue && pa.DateRens.Value >= oneYearAgo)
                                .Sum(pa => (decimal?)pa.MontantEncais) ?? 0

                            let appointments = _context.RendezVous
                                .Where(r => r.IdPersonne == patient.IdPersonne)

                            let lastAppointment = appointments
                                .OrderByDescending(r => r.RdvDate)
                                .Select(r => r.RdvDate)
                                .FirstOrDefault()

                            let appointmentCountLastYear = appointments
                                .Count(r => r.RdvDate >= oneYearAgo)

                            where totalRevenue >= vipMinTotalRevenue || annualRevenue >= vipMinAnnualRevenue

                            let daysSinceLastVisit = lastAppointment != null
                                ? (int)(DateTime.Now - lastAppointment).TotalDays
                                : (int?)null

                            let monthsSinceLastVisit = daysSinceLastVisit.HasValue
                                ? daysSinceLastVisit.Value / 30
                                : (int?)null

                            let isAtRisk = totalRevenue >= vipMinTotalRevenue &&
                                          (lastAppointment == null || lastAppointment < cutoffDate)

                            let isActive = totalRevenue >= vipMinTotalRevenue &&
                                          lastAppointment != null &&
                                          lastAppointment >= cutoffDate

                            select new VipPatientDto
                            {
                                PatientId = patient.IdPersonne,
                                LastName = patient.Personne.PerNom,
                                FirstName = patient.Personne.PerPrenom ?? "",
                                Phone = patient.Personne.PerTelPrinc ?? patient.Personne.Gsm,
                                Email = patient.Personne.PerEmail,

                                TotalRevenue = totalRevenue,
                                AnnualRevenue = annualRevenue,

                                LastAppointmentDate = lastAppointment,
                                DaysSinceLastVisit = daysSinceLastVisit,
                                MonthsSinceLastVisit = monthsSinceLastVisit,
                                LastVisitDisplay = lastAppointment != null
                                    ? $"Il y a {monthsSinceLastVisit ?? 0} mois"
                                    : "Aucune visite",

                                VisitFrequency = appointmentCountLastYear,

                                IsAtRisk = isAtRisk,
                                IsActive = isActive,
                                Status = isAtRisk ? "À risque" : "VIP Actif"
                            };

                if (!string.IsNullOrEmpty(filterParams.Status))
                {
                    query = query.Where(p => p.Status == filterParams.Status);
                }

                if (filterParams.MinRevenue.HasValue)
                {
                    query = query.Where(p => p.TotalRevenue >= filterParams.MinRevenue.Value);
                }

                if (filterParams.MaxRevenue.HasValue)
                {
                    query = query.Where(p => p.TotalRevenue <= filterParams.MaxRevenue.Value);
                }

                if (!string.IsNullOrEmpty(filterParams.SearchQuery))
                {
                    var searchLower = filterParams.SearchQuery.ToLower();
                    query = query.Where(p =>
                        p.LastName.ToLower().Contains(searchLower) ||
                        (p.FirstName != null && p.FirstName.ToLower().Contains(searchLower)) ||
                        (p.Phone != null && p.Phone.Contains(searchLower)) ||
                        (p.Email != null && p.Email.ToLower().Contains(searchLower))
                    );
                }

                query = filterParams.SortBy?.ToLower() switch
                {
                    "revenue_desc" => query.OrderByDescending(x => x.TotalRevenue),
                    "revenue_asc" => query.OrderBy(x => x.TotalRevenue),
                    "last_visit_oldest" => query.OrderBy(x => x.LastAppointmentDate ?? DateTime.MinValue),
                    "last_visit_newest" => query.OrderByDescending(x => x.LastAppointmentDate ?? DateTime.MinValue),
                    "name_asc" => query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName),
                    _ => query.OrderByDescending(x => x.IsAtRisk).ThenByDescending(x => x.TotalRevenue)
                };

                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                    .Take(filterParams.PageSize)
                    .ToListAsync();

                return new PagedResponse<VipPatientDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving VIP patients: {ex.Message}", ex);
            }
        }
        // Keep the non-paginated version for backward compatibility
        public async Task<List<VipPatientDto>> GetVipPatientsDetailed()
        {
            var allPatients = await GetVipPatientsPaginated(new VipPatientFilterParams
            {
                PageNumber = 1,
                PageSize = int.MaxValue
            });

            return allPatients.Data;
        }
        public async Task<int> GetLostPatients()
        {
            try
            {
                int monthsWithoutAppointment = (await _context.KpiConfigs.FirstOrDefaultAsync(e => e.KpiCode == "vip_last_visit_months")) is KpiConfig config ? (int)config.TargetValue! : 12;
                var cutoffDate = DateTime.Now.AddMonths(-monthsWithoutAppointment);
                var patientsWithRecentAppointments = await _context.RendezVous
        .Where(rdv => rdv.RdvDate >= cutoffDate)
        .Distinct()
        .CountAsync();

                // Total patients - Active patients = Lost patients
                var totalPatients = await _context.Patients
                    .Where(p => p.IdPersonne > 0)
                    .CountAsync();

                return totalPatients - patientsWithRecentAppointments;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public async Task<int> GetTotalActivePatients()
        {
            try
            {
                var activePatients = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.Personne.IdPersonne > 0)
                    .CountAsync();
                return activePatients;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private PatientCreationGrouping DetermineGroupingLevel(TimeSpan dateSpan)
        {
            if (dateSpan.TotalDays > 4 * 365) // More than 4 years
                return PatientCreationGrouping.Year;
            else if (dateSpan.TotalDays > 4 * 30) // More than 4 months
                return PatientCreationGrouping.Month;
            else
                return PatientCreationGrouping.Day;
        }

        private async Task<List<PatientCreationStatistic>> GetPatientCreationDataFromDbAsync(
            DateOnly fromDate, DateOnly toDate, PatientCreationGrouping grouping)
        {
            try
            {
                var query = _context.Patients
                    .WithPersonne()
                    .Where(p => p.PatDateCreation != null &&
                               p.PatDateCreation >= fromDate &&
                               p.PatDateCreation <= toDate &&
                               p.Personne.IdPersonne > 0);

                List<PatientCreationStatistic> results;

                switch (grouping)
                {
                    case PatientCreationGrouping.Day:
                        var dayData = await query
                            .GroupBy(p => p.PatDateCreation.Value)
                            .Select(g => new
                            {
                                DateOnly = g.Key,
                                Count = g.Count()
                            })
                            .OrderBy(x => x.DateOnly)
                            .ToListAsync();

                        results = dayData.Select(d => new PatientCreationStatistic
                        {
                            Date = d.DateOnly.ToDateTime(TimeOnly.MinValue),
                            Count = d.Count,
                            GroupingLevel = grouping,
                            PeriodLabel = d.DateOnly.ToString("yyyy-MM-dd")
                        }).ToList();
                        break;

                    case PatientCreationGrouping.Month:
                        var monthData = await query
                            .GroupBy(p => new { p.PatDateCreation.Value.Year, p.PatDateCreation.Value.Month })
                            .Select(g => new
                            {
                                g.Key.Year,
                                g.Key.Month,
                                Count = g.Count()
                            })
                            .OrderBy(x => x.Year).ThenBy(x => x.Month)
                            .ToListAsync();

                        results = monthData.Select(d => new PatientCreationStatistic
                        {
                            Date = new DateTime(d.Year, d.Month, 1),
                            Count = d.Count,
                            GroupingLevel = grouping,
                            PeriodLabel = $"{d.Year:0000}-{d.Month:00}"
                        }).ToList();
                        break;

                    case PatientCreationGrouping.Year:
                        var yearData = await query
                            .GroupBy(p => p.PatDateCreation.Value.Year)
                            .Select(g => new
                            {
                                Year = g.Key,
                                Count = g.Count()
                            })
                            .OrderBy(x => x.Year)
                            .ToListAsync();

                        results = yearData.Select(d => new PatientCreationStatistic
                        {
                            Date = new DateTime(d.Year, 1, 1),
                            Count = d.Count,
                            GroupingLevel = grouping,
                            PeriodLabel = d.Year.ToString()
                        }).ToList();
                        break;

                    default:
                        throw new ArgumentException("Invalid grouping level");
                }

                return results;
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error in GetPatientCreationDataFromDbAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get patient engagement statistics including appointments and treatment realization
        /// </summary>
        public async Task<PatientEngagementStatistics> GetPatientEngagementStatisticsAsync()
        {
            try
            {
                var stats = new PatientEngagementStatistics();

                // Get total patient count
                var totalPatients = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.Personne.IdPersonne > 0)
                    .CountAsync();

                stats.TotalPatients = totalPatients;

                if (totalPatients == 0)
                {
                    return stats;
                }

                // 1. Patients with no appointments at all
                var patientsWithNoAppointments = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.Personne.IdPersonne > 0)
                    .Where(p => !_context.RendezVous.Any(rdv => rdv.IdPersonne == p.IdPersonne))
                    .CountAsync();

                stats.PatientsWithNoAppointments = patientsWithNoAppointments;
                stats.PatientsWithNoAppointmentsPercentage = CalculatePercentage(patientsWithNoAppointments, totalPatients);

                // 2. Patients with at least one appointment but never showed up
                var patientsWithAppointmentsButNeverShowedUp = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.Personne.IdPersonne > 0)
                    .Where(p => _context.RendezVous.Any(rdv => rdv.IdPersonne == p.IdPersonne))
                    .Where(p => !_context.RendezVous.Any(rdv => rdv.IdPersonne == p.IdPersonne && rdv.RdvStatut == 1))
                    .CountAsync();

                stats.PatientsWithAppointmentsButNeverShowedUp = patientsWithAppointmentsButNeverShowedUp;
                stats.PatientsWithAppointmentsButNeverShowedUpPercentage = CalculatePercentage(patientsWithAppointmentsButNeverShowedUp, totalPatients);

                // 3. Patients with no realized treatments
                var patientsWithNoRealizedTreatments = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.Personne.IdPersonne > 0)
                    .Where(p => !_context.DentalisActesPatient.Any(acte =>
                        acte.ApPatient == p.IdPersonne &&
                        acte.ApRealise == 1))
                    .CountAsync();

                stats.PatientsWithNoRealizedTreatments = patientsWithNoRealizedTreatments;
                stats.PatientsWithNoRealizedTreatmentsPercentage = CalculatePercentage(patientsWithNoRealizedTreatments, totalPatients);

                // 4. Patients with at least one realized treatment
                var patientsWithRealizedTreatments = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.Personne.IdPersonne > 0)
                    .Where(p => _context.DentalisActesPatient.Any(acte =>
                        acte.ApPatient == p.IdPersonne &&
                        acte.ApRealise == 1))
                    .CountAsync();

                stats.PatientsWithRealizedTreatments = patientsWithRealizedTreatments;
                stats.PatientsWithRealizedTreatmentsPercentage = CalculatePercentage(patientsWithRealizedTreatments, totalPatients);

                return stats;
            }
            catch (Exception)
            {
                return new PatientEngagementStatistics();
            }
        }

        /// <summary>
        /// Get detailed patient engagement statistics - FIREBIRD SAFE VERSION
        /// Uses only simple SELECT queries with no EXISTS, no ANY, no subqueries
        /// </summary>
        public async Task<DetailedPatientEngagementStatistics> GetDetailedPatientEngagementStatisticsAsync()
        {
            try
            {
                var stats = new DetailedPatientEngagementStatistics();

                // Get total patient count
                var totalPatients = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.Personne.IdPersonne > 0)
                    .CountAsync();

                stats.TotalPatients = totalPatients;

                if (totalPatients == 0)
                {
                    return stats;
                }

                // Step 1: Get all patient IDs
                var allPatientIds = await _context.Patients
                    .WithPersonne()
                    .Where(p => p.Personne.IdPersonne > 0)
                    .Select(p => p.IdPersonne)
                    .ToListAsync();

                var patientIdSet = new HashSet<int>(allPatientIds);

                // Step 2: Get distinct patient IDs with any appointment
                var patientsWithAppointments = await _context.RendezVous
                    .Where(rdv => patientIdSet.Contains(rdv.IdPersonne))
                    .Select(rdv => rdv.IdPersonne)
                    .Distinct()
                    .ToListAsync();
                var patientsWithAppointmentsSet = new HashSet<int>(patientsWithAppointments);

                // Step 3: Get distinct patient IDs who showed up (RdvStatut = 1)
                var patientsWhoShowedUp = await _context.RendezVous
                    .Where(rdv => patientIdSet.Contains(rdv.IdPersonne) && rdv.RdvStatut == 1)
                    .Select(rdv => rdv.IdPersonne)
                    .Distinct()
                    .ToListAsync();
                var patientsWhoShowedUpSet = new HashSet<int>(patientsWhoShowedUp);

                // Step 4: Get distinct patient IDs with cancelled appointments (RdvStatut = 2)
                var patientsWithCancelled = await _context.RendezVous
                    .Where(rdv => patientIdSet.Contains(rdv.IdPersonne) && rdv.RdvStatut == 2)
                    .Select(rdv => rdv.IdPersonne)
                    .Distinct()
                    .ToListAsync();
                var patientsWithCancelledSet = new HashSet<int>(patientsWithCancelled);

                // Step 5: Get distinct patient IDs with no-show appointments (RdvStatut = 3)
                var patientsWithNoShow = await _context.RendezVous
                    .Where(rdv => patientIdSet.Contains(rdv.IdPersonne) && rdv.RdvStatut == 3)
                    .Select(rdv => rdv.IdPersonne)
                    .Distinct()
                    .ToListAsync();
                var patientsWithNoShowSet = new HashSet<int>(patientsWithNoShow);

                // Step 6: Get distinct patient IDs with realized treatments (ApRealise = 1)
                var patientsWithRealizedTreatments = await _context.DentalisActesPatient
                    .Where(acte => patientIdSet.Contains(acte.ApPatient) && acte.ApRealise == 1)
                    .Select(acte => acte.ApPatient)
                    .Distinct()
                    .ToListAsync();
                var patientsWithRealizedTreatmentsSet = new HashSet<int>(patientsWithRealizedTreatments);

                // Step 7: Get distinct patient IDs with planned treatments (ApRealise = 0)
                var patientsWithPlannedTreatments = await _context.DentalisActesPatient
                    .Where(acte => patientIdSet.Contains(acte.ApPatient) && acte.ApRealise == 0)
                    .Select(acte => acte.ApPatient)
                    .Distinct()
                    .ToListAsync();
                var patientsWithPlannedTreatmentsSet = new HashSet<int>(patientsWithPlannedTreatments);

                // Calculate all statistics in memory using HashSet operations

                // 1. No appointments
                stats.PatientsWithNoAppointments = allPatientIds.Count(id => !patientsWithAppointmentsSet.Contains(id));
                stats.PatientsWithNoAppointmentsPercentage = CalculatePercentage(stats.PatientsWithNoAppointments, totalPatients);

                // 2. Has appointments but never showed up
                stats.PatientsWithAppointmentsButNeverShowedUp = patientsWithAppointments.Count(id => !patientsWhoShowedUpSet.Contains(id));
                stats.PatientsWithAppointmentsButNeverShowedUpPercentage = CalculatePercentage(stats.PatientsWithAppointmentsButNeverShowedUp, totalPatients);

                // 3. No realized treatments
                stats.PatientsWithNoRealizedTreatments = allPatientIds.Count(id => !patientsWithRealizedTreatmentsSet.Contains(id));
                stats.PatientsWithNoRealizedTreatmentsPercentage = CalculatePercentage(stats.PatientsWithNoRealizedTreatments, totalPatients);

                // 4. Has realized treatments
                stats.PatientsWithRealizedTreatments = patientsWithRealizedTreatments.Count;
                stats.PatientsWithRealizedTreatmentsPercentage = CalculatePercentage(stats.PatientsWithRealizedTreatments, totalPatients);

                // 5. Planned but not realized treatments
                stats.PatientsWithPlannedButNotRealizedTreatments = patientsWithPlannedTreatments.Count(id => !patientsWithRealizedTreatmentsSet.Contains(id));
                stats.PatientsWithPlannedButNotRealizedTreatmentsPercentage = CalculatePercentage(stats.PatientsWithPlannedButNotRealizedTreatments, totalPatients);

                // 6. No-show appointments
                stats.PatientsWithNoShowAppointments = patientsWithNoShow.Count;
                stats.PatientsWithNoShowAppointmentsPercentage = CalculatePercentage(stats.PatientsWithNoShowAppointments, totalPatients);

                // 7. Cancelled appointments
                stats.PatientsWithCancelledAppointments = patientsWithCancelled.Count;
                stats.PatientsWithCancelledAppointmentsPercentage = CalculatePercentage(stats.PatientsWithCancelledAppointments, totalPatients);

                return stats;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error in GetDetailedPatientEngagementStatisticsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new DetailedPatientEngagementStatistics();
            }
        }

        /// <summary>
        /// Get basic patient engagement statistics - FIREBIRD SAFE VERSION
        /// </summary>


        private decimal CalculatePercentage(int count, int total)
        {
            if (total == 0) return 0;
            return Math.Round((decimal)count / total * 100, 2);
        }

        private List<PatientCreationStatistic> FillPatientCreationGaps(
            List<PatientCreationStatistic> dbResults, DateTime fromDate, DateTime toDate,
            PatientCreationGrouping grouping)
        {
            var filledResults = new List<PatientCreationStatistic>();
            var dbResultsDict = dbResults.ToDictionary(x => x.Date, x => x);

            DateTime currentDate = GetPeriodStart(fromDate, grouping);
            DateTime endDate = GetPeriodStart(toDate, grouping);

            while (currentDate <= endDate)
            {
                if (dbResultsDict.TryGetValue(currentDate, out var existingStat))
                {
                    filledResults.Add(existingStat);
                }
                else
                {
                    // Fill gap with zero count
                    filledResults.Add(new PatientCreationStatistic
                    {
                        Date = currentDate,
                        Count = 0,
                        GroupingLevel = grouping,
                        PeriodLabel = FormatPeriodLabel(currentDate, grouping)
                    });
                }

                currentDate = GetNextPeriod(currentDate, grouping);
            }

            return filledResults;
        }

        private DateTime GetPeriodStart(DateTime date, PatientCreationGrouping grouping)
        {
            return grouping switch
            {
                PatientCreationGrouping.Day => date.Date,
                PatientCreationGrouping.Month => new DateTime(date.Year, date.Month, 1),
                PatientCreationGrouping.Year => new DateTime(date.Year, 1, 1),
                _ => date.Date
            };
        }

        private DateTime GetNextPeriod(DateTime currentDate, PatientCreationGrouping grouping)
        {
            return grouping switch
            {
                PatientCreationGrouping.Day => currentDate.AddDays(1),
                PatientCreationGrouping.Month => currentDate.AddMonths(1),
                PatientCreationGrouping.Year => currentDate.AddYears(1),
                _ => currentDate.AddDays(1)
            };
        }

        private string FormatPeriodLabel(DateTime date, PatientCreationGrouping grouping)
        {
            return grouping switch
            {
                PatientCreationGrouping.Day => date.ToString("yyyy-MM-dd"),
                PatientCreationGrouping.Month => date.ToString("yyyy-MM"),
                PatientCreationGrouping.Year => date.Year.ToString(),
                _ => date.ToString("yyyy-MM-dd")
            };
        }

        #endregion
    }


}