using Microsoft.EntityFrameworkCore;
using EvoConnect.Server.Data;
using EvoConnect.Server.DTOs;
using EvoConnect.Server.Repository.Interfaces;

namespace EvoConnect.Server.Repository
{
    public class ActesRepository(ClinicDbContext context) : IActesRepository
    {
        private readonly ClinicDbContext _context = context;

        public async Task<PaginatedResult<DentalisActeDto>> GetAllActesAsync(int pageNumber, int pageSize)
        {
            var query = _context.DentalisActes
                .Include(a => a.Famille)
                .Include(a => a.CodeSecu)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var actes = await query
                .OrderBy(a => a.ActePk)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new DentalisActeDto
                {
                    ActePk = a.ActePk,
                    ActeNom = a.ActeNom,
                    ActeAbrege = a.ActeAbrege,
                    ActeFamille = a.ActeFamille,
                    FamilleNom = a.Famille != null ? a.Famille.FacteNom : null,
                    ActeCodeSecu = a.ActeCodeSecu,
                    CodeSecu = a.CodeSecu != null ? new CodeSecuDto
                    {
                        IdCodeSecu = a.CodeSecu.IdCodeSecu,
                        CodeNom = a.CodeSecu.CodeNom,
                        CodeLibelle = a.CodeSecu.CodeLibelle,
                        CodeValeur = a.CodeSecu.CodeValeur,
                        CodeValeurEuro = a.CodeSecu.CodeValeurEuro,
                        TauxRembours = a.CodeSecu.TauxRembours,
                        CodePrestation = a.CodeSecu.CodePrestation,
                        CodeCoeff = a.CodeSecu.CodeCoeff
                    } : null,
                    ActeNbDents = a.ActeNbDents,
                    ActeMontantA = a.ActeMontantA,
                    ActeMontantE = a.ActeMontantE,
                    ActeMontantU = a.ActeMontantU,
                    ActeDateCreation = a.ActeDateCreation,
                    ActeDateMaj = a.ActeDateMaj
                })
                .ToListAsync();

            return new PaginatedResult<DentalisActeDto>
            {
                Items = actes,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = totalPages,
                    HasNext = pageNumber < totalPages,
                    HasPrevious = pageNumber > 1
                }
            };
        }

        public async Task<List<ActePatientDto>> GetActesByPatientIdAsync(int patientId)
        {
            // Load entities first - avoids character set transliteration in SQL
            var actesPatient = await _context.DentalisActesPatient
                .Include(ap => ap.Acte)
                 .ThenInclude(ap => ap.CodeSecu)
                .Where(ap => ap.ApPatient == patientId && ap.ApActe > 0)
                .AsNoTracking()
                .ToListAsync();

            // Project in-memory where C# handles character encoding
            return actesPatient.Select(ap => new ActePatientDto
            {
                ApPk = ap.ApPk,
                ApPatient = ap.ApPatient,
                ApActe = ap.ApActe,
                ActeNom = ap.Acte?.ActeNom,
                ApLibelleActe = ap.ApLibelleActe,
                ApRealise = ap.ApRealise,
                ApDateRealise = ap.ApDateRealise,
                ApDentFrom = ap.ApDentFrom,
                ApDentTo = ap.ApDentTo,
                ApSelectedDents = ap.ApSelectedDents,
                ApMontant = ap.ApMontant,
                CodeSecu = ap.Acte?.CodeSecu == null ? null : new CodeSecuDto
                {
                    IdCodeSecu = ap.Acte.CodeSecu.IdCodeSecu,
                    CodeNom = ap.Acte.CodeSecu.CodeNom,
                    CodeLibelle = ap.Acte.CodeSecu.CodeLibelle,
                    CodeValeur = ap.Acte.CodeSecu.CodeValeur,
                    CodeValeurEuro = ap.Acte.CodeSecu.CodeValeurEuro,
                    TauxRembours = ap.Acte.CodeSecu.TauxRembours,
                    CodePrestation = ap.Acte.CodeSecu.CodePrestation,
                    CodeCoeff = ap.Acte.CodeSecu.CodeCoeff
                },
                ApDateCreation = ap.ApDateCreation,
                ApFinalise = ap.ApFinalise,
                ApDateFinalise = ap.ApDateFinalise,
                ApFacture = ap.ApFacture,
                ApFactureDate = ap.ApFactureDate
            }).ToList();
        }

        public async Task<DentalisActeDto?> GetActeByIdAsync(int acteId)
        {
            return await _context.DentalisActes
                .Include(a => a.Famille)
                .Include(a => a.CodeSecu)
                .Where(a => a.ActePk == acteId)
                .AsNoTracking()
                .Select(a => new DentalisActeDto
                {
                    ActePk = a.ActePk,
                    ActeNom = a.ActeNom,
                    ActeAbrege = a.ActeAbrege,
                    ActeFamille = a.ActeFamille,
                    FamilleNom = a.Famille != null ? a.Famille.FacteNom : null,
                    ActeCodeSecu = a.ActeCodeSecu,
                    CodeSecu = a.CodeSecu != null ? new CodeSecuDto
                    {
                        IdCodeSecu = a.CodeSecu.IdCodeSecu,
                        CodeNom = a.CodeSecu.CodeNom,
                        CodeLibelle = a.CodeSecu.CodeLibelle,
                        CodeValeur = a.CodeSecu.CodeValeur,
                        CodeValeurEuro = a.CodeSecu.CodeValeurEuro,
                        TauxRembours = a.CodeSecu.TauxRembours,
                        CodePrestation = a.CodeSecu.CodePrestation,
                        CodeCoeff = a.CodeSecu.CodeCoeff
                    } : null,
                    ActeNbDents = a.ActeNbDents,
                    ActeMontantA = a.ActeMontantA,
                    ActeMontantE = a.ActeMontantE,
                    ActeMontantU = a.ActeMontantU,
                    ActeDateCreation = a.ActeDateCreation,
                    ActeDateMaj = a.ActeDateMaj
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ActePatientDto?> GetActePatientByIdAsync(int apPk)
        {
            return await _context.DentalisActesPatient
                .Include(ap => ap.Acte)
                .Include(ap => ap.CodeSecu)
                .Where(ap => ap.ApPk == apPk)
                .AsNoTracking()
                .Select(ap => new ActePatientDto
                {
                    ApPk = ap.ApPk,
                    ApPatient = ap.ApPatient,
                    ApActe = ap.ApActe,
                    ActeNom = ap.Acte != null ? ap.Acte.ActeNom : null,
                    ApLibelleActe = ap.ApLibelleActe,
                    ApRealise = ap.ApRealise,
                    ApDateRealise = ap.ApDateRealise,
                    ApDentFrom = ap.ApDentFrom,
                    ApDentTo = ap.ApDentTo,
                    ApSelectedDents = ap.ApSelectedDents,
                    ApMontant = ap.ApMontant,
                    CodeSecu = ap.CodeSecu != null ? new CodeSecuDto
                    {
                        IdCodeSecu = ap.CodeSecu.IdCodeSecu,
                        CodeNom = ap.CodeSecu.CodeNom,
                        CodeLibelle = ap.CodeSecu.CodeLibelle,
                        CodeValeur = ap.CodeSecu.CodeValeur,
                        CodeValeurEuro = ap.CodeSecu.CodeValeurEuro,
                        TauxRembours = ap.CodeSecu.TauxRembours,
                        CodePrestation = ap.CodeSecu.CodePrestation,
                        CodeCoeff = ap.CodeSecu.CodeCoeff
                    } : null,
                    ApDateCreation = ap.ApDateCreation,
                    ApFinalise = ap.ApFinalise,
                    ApDateFinalise = ap.ApDateFinalise,
                    ApFacture = ap.ApFacture,
                    ApFactureDate = ap.ApFactureDate
                })
                .FirstOrDefaultAsync();
        }

        public Task<int> GetTodayRealisedActesCount()
        {
            var Today = DateTime.Today;
            var Tomorrow = Today.AddDays(1);
            return _context.DentalisActesPatient
             .Where(ap => ap.ApRealise == 1 && ap.ApDateRealise >= Today && ap.ApDateRealise < Tomorrow).CountAsync();

        }
        public async Task<float> GetTodayRealisedActesEncaiss()
        {
            var Today = DateTime.Today;
            var Tomorrow = Today.AddDays(1);
            var query = _context.DentalisActesPatient
             .Where(ap => ap.ApRealise == 1 && ap.ApDateRealise >= Today && ap.ApDateRealise < Tomorrow);
           
            return await query.SumAsync(e => e.ApMontant);

        }
    }
}