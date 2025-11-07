
using EvoConnect.Server.Data;
using EvoConnect.Server.Models;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvoConnect.Server.Repository
{
    public class AppointmentsDA : IAppointmentsDA
    {
        private readonly ClinicDbContext _context;

        public AppointmentsDA(ClinicDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.RendezVous
                .Include(r => r.Patient.Personne)
                .Include(r => r.Professionnel)
                .Include(r => r.Acte)
               
                .Where(r => r.RdvDate >= fromDate && r.RdvDate <= toDate)
                .OrderBy(r => r.RdvDate)
                .Select(r => new AppointmentDto
                {
                    Id = r.IdRdv,
                    PatientId = r.IdPersonne,
                    PraticienId = r.PerIdPersonne,
                    ActeId = r.IdActe,
                    FauteuilId = r.IdFauteuil,
                    DateRendezVous = r.RdvDate,
                    Duree = r.RdvDuree,
                    Statut = r.RdvStatut,
                    HeureArrivee = r.RdvArrivee,
                    Commentaire = r.RdvComm ?? "",
                    CommentaireInternet = r.RdvCommInternet ?? "",
                    LastModified = r.LastModif,
                    HeureFauteuil = r.HeureFauteuil,
                    HeureSalleAttente = r.HeureSalleAttente,
                    HeureSecretariat = r.HeureSecretariat,
                    HeureSortie = r.HeureSorti,
                    StatutConfirmation = r.StatutConfirmation ?? "",
                    PatientNom = r.Patient.Personne.PerNom,
                    PatientPrenom = r.Patient.Personne.PerPrenom,
                    PatientTelephone = r.Patient.Personne.PerTelPrinc ?? "",
                    ActeLibelle = r.Acte.ActeLibelle,
                    ActeCouleur = r.Acte.ActeCouleur,
                    TypeActe = r.Acte.TypeActe,
                    FauteuilLibelle = _context.Fauteuils.FirstOrDefault(e=>e.IdFauteuil == r.IdFauteuil).FautLibelle

                })
                .ToListAsync();
        }

        public async Task<List<MonthlyAppointmentStats>> GetMonthlyAppointmentStatsAsync(int year)
        {
            return await _context.RendezVous
                .Where(r => r.RdvDate.Year == year)
                .GroupBy(r => new { r.RdvDate.Year, r.RdvDate.Month })
                .Select(g => new MonthlyAppointmentStats
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(x => x.RdvStatut == 1),
                    CancelledAppointments = g.Count(x => x.Localisation == 5),
                    AverageDuration = g.Average(x => x.RdvDuree)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();
        }

        public async Task<List<ActeStats>> GetAppointmentsByActeAsync(DateTime fromDate, DateTime toDate)
        {

            return await _context.RendezVous
                .Where(r => r.RdvDate >= fromDate && r.RdvDate <= toDate)
                .Where(r => r.Acte != null) // Ensure Acte is not null
                .GroupBy(r => new
                {
                    r.Acte.IdActe,
                    r.Acte.ActeLibelle,
                    r.Acte.ActeCouleur,
                    r.Acte.TypeActe
                })
                .Select(g => new ActeStats
                {
                    ActeId = g.Key.IdActe,
                    ActeLibelle = g.Key.ActeLibelle,
                    ActeCouleur = g.Key.ActeCouleur,
                    TypeActe = g.Key.TypeActe ?? 0,
                    TotalAppointments = g.Count(),
                    AverageDuration = g.Average(r => r.RdvDuree),
                    TotalDuration = g.Sum(r => r.RdvDuree)
                })
                .OrderByDescending(x => x.TotalAppointments)
                .ToListAsync();


        }

        public async Task<List<FauteuilStats>> GetAppointmentsByFauteuilAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {

                return await _context.RendezVous
    .Where(r => r.IdRdv > 0 && r.RdvDate >= fromDate && r.RdvDate <= toDate)
    .Where(r => _context.Fauteuils.Any(f => f.IdFauteuil == r.IdFauteuil)) // Ensure Fauteuil exists
    .Join(_context.Fauteuils,
          r => r.IdFauteuil,
          f => f.IdFauteuil,
          (r, f) => new { RendezVous = r, Fauteuil = f })
    .GroupBy(x => new
    {
        x.Fauteuil.IdFauteuil,
        x.Fauteuil.FautLibelle,
        x.Fauteuil.FautPraticien
    })
    .Select(g => new FauteuilStats
    {
        FauteuilId = g.Key.IdFauteuil,
        FauteuilLibelle = g.Key.FautLibelle,
        PraticienId = g.Key.FautPraticien,
        TotalAppointments = g.Count(),
        AverageDuration = g.Average(x => (double)x.RendezVous.RdvDuree),
        TotalDuration = g.Sum(x => x.RendezVous.RdvDuree)
    })
    .OrderByDescending(x => x.TotalAppointments)
    .ToListAsync();
            }
            catch (System.Exception r)
            {

                throw;
            }
        }

        public async Task<AppointmentSummaryStats> GetAppointmentSummaryStatsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {


                var query = _context.RendezVous
        .Where(r => r.RdvDate >= fromDate && r.RdvDate <= toDate);

                return await query.GroupBy(r => 1).Select(g => new AppointmentSummaryStats
                {
                    TotalAppointments = g.Count(),
                    UniquePatients = g.Select(x => x.IdPersonne).Distinct().Count(),
                    UniqueActes = g.Select(x => x.IdActe).Distinct().Count(),
                    UniqueFauteuils = g.Select(x => x.IdFauteuil).Distinct().Count(),
                    AverageDuration = g.Average(x => x.RdvDuree),
                    TotalDuration = g.Sum(x => x.RdvDuree),
                    CompletedAppointments = g.Count(x => x.RdvStatut == 1),
                    CancelledAppointments = g.Count(x => x.RdvStatut == 0),
                    ArrivedAppointments = g.Count(x => x.RdvArrivee.Value.Date == DateTime.Today)
                }).FirstOrDefaultAsync() ?? new AppointmentSummaryStats();
            }
            catch (System.Exception r)
            {

                throw;
            }
        }

        public async Task<List<DailyAppointmentStats>> GetDailyAppointmentStatsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {


                return await _context.RendezVous
          .Where(r => r.RdvDate >= fromDate && r.RdvDate <= toDate)
          .GroupBy(r => r.RdvDate.Date)
          .Select(g => new DailyAppointmentStats
          {
              Date = g.Key,
              TotalAppointments = g.Count(),
              CompletedAppointments = g.Count(x => x.RdvStatut == 1),
              CancelledAppointments = g.Count(x => x.RdvStatut == 0),
              ArrivedAppointments = g.Count(x => x.RdvArrivee.Value.Date == DateTime.Today),
              AverageDuration = g.Average(x => x.RdvDuree)
          })
          .OrderBy(x => x.Date)
          .ToListAsync();
            }
            catch (System.Exception r)
            {

                throw;
            }
        }

        public async Task<List<AppointmentDto>> GetUpcomingAppointmentsAsync(int days = 7)
        {
            var fromDate = DateTime.Now;
            var toDate = DateTime.Now.AddDays(days);
        try
        {
            
      
            return await _context.RendezVous
                .Include(r => r.Patient.Personne)
                .Include(r => r.Acte)
       
                .Where(r => r.RdvDate >= fromDate && r.RdvDate <= toDate && r.RdvStatut != 0)
                .OrderBy(r => r.RdvDate)
                .Select(r => new AppointmentDto
                {
                    Id = r.IdRdv,
                    PatientId = r.IdPersonne,
                    PraticienId = r.PerIdPersonne,
                    ActeId = r.IdActe,
                    FauteuilId = r.IdFauteuil,
                    DateRendezVous = r.RdvDate,
                    Duree = r.RdvDuree,
                    Statut = r.RdvStatut,
                    HeureArrivee = r.RdvArrivee,
                    Commentaire = r.RdvComm ?? "",
                    CommentaireInternet = r.RdvCommInternet ?? "",
                    LastModified = r.LastModif,
                    HeureFauteuil = r.HeureFauteuil,
                    HeureSalleAttente = r.HeureSalleAttente,
                    HeureSecretariat = r.HeureSecretariat,
                    HeureSortie = r.HeureSorti,
                    StatutConfirmation = r.StatutConfirmation ?? "",
                    PatientNom = r.Patient.Personne.PerNom,
                    PatientPrenom = r.Patient.Personne.PerPrenom,
                    PatientTelephone = r.Patient.Personne.PerTelPrinc ?? "",
                    ActeLibelle = r.Acte.ActeLibelle,
                    ActeCouleur = r.Acte.ActeCouleur,
                    TypeActe = r.Acte.TypeActe,
                    FauteuilLibelle = _context.Fauteuils.FirstOrDefault(e=>e.IdFauteuil == r.IdFauteuil).FautLibelle
                })
                .ToListAsync();  }
        catch (System.Exception)
        {
            
            throw;
        }
        }

        public async Task<List<ActeDto>> GetAllActesAsync()
        {
            return await _context.Actes
                .OrderBy(a => a.ActeLibelle)
                .Select(a => new ActeDto
                {
                    Id = a.IdActe,
                    Libelle = a.ActeLibelle,
                    DureeStandard = a.ActeDurestd,
                    Couleur = a.ActeCouleur,
                    TypeActe = a.TypeActe,
                    NbFauteuilBloc = a.NbFautBloc,
                    TempsChrono = a.TempsChrono,
                    AnticipationRdv = a.AnticipRdv,
                    Courrier = a.Courrier,
                    AnticipationSms = a.AnticipationSms,
                    CategorieStats = a.CategStats,
                    Tarif = a.Tarif
                })
                .ToListAsync();
        }

        public async Task<List<FauteuilDto>> GetAllFauteuilsAsync()
        {
            return await _context.Fauteuils
                .OrderBy(f => f.FautLibelle)
                .Select(f => new FauteuilDto
                {
                    Id = f.IdFauteuil,
                    Libelle = f.FautLibelle,
                    PraticienId = f.FautPraticien
                })
                .ToListAsync();
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
        public async Task<List<AppointmentStats>> GetAppointmentStatsAsync(DateTime startDate, DateTime endDate)
        {
            var groupingType = DetermineGroupingType(startDate, endDate);
            var query = _context.RendezVous
                .Where(r => r.RdvDate >= startDate && r.RdvDate <= endDate);

            var rawStats = groupingType switch
            {
                GroupingType.Hour => await GroupByHour(query),
                GroupingType.Day => await GroupByDay(query),
                GroupingType.Month => await GroupByMonth(query),
                GroupingType.Year => await GroupByYear(query),
                _ => await GroupByYear(query)
            };

            return FillGaps(rawStats, startDate, endDate, groupingType);
        }


        private async Task<List<AppointmentStats>> GroupByHour(IQueryable<RendezVous> query)
        {
        var results = await query
            .GroupBy(r => new
            {
                Year = r.RdvDate.Year,
                Month = r.RdvDate.Month,
                Day = r.RdvDate.Day,
                Hour = r.RdvDate.Hour
            })
            .Select(g => new 
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                g.Key.Hour,
                TotalAppointments = g.Count(),
                CompletedAppointments = g.Count(x => x.RdvStatut == 1),
                CancelledAppointments = g.Count(x => x.Localisation == 5),
                ArrivedAppointments = g.Count(x => x.RdvArrivee.Value.Date == DateTime.Today),
                AverageDuration = g.Average(x => (double?)x.RdvDuree) ?? 0
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day).ThenBy(x => x.Hour)
            .ToListAsync();

            return [.. results.Select(r => new AppointmentStats
            {
                Year = (int)r.Year,
                Month = (int)r.Month,
                Day = (int)r.Day,
                Hour = (int)r.Hour,
                Period = $"{r.Year:0000}-{r.Month:00}-{r.Day:00} {r.Hour:00}:00",
                TotalAppointments = r.TotalAppointments,
                CompletedAppointments = r.CompletedAppointments,
                CancelledAppointments = r.CancelledAppointments,
                 ArrivedAppointments = r.ArrivedAppointments,

                AverageDuration = r.AverageDuration,
                GroupingType = GroupingType.Hour
            })];
        }

        private async Task<List<AppointmentStats>> GroupByDay(IQueryable<RendezVous> query)
        {
            var results = await query
                .GroupBy(r => new
                {
                    Year = r.RdvDate.Year,
                    Month = r.RdvDate.Month,
                    Day = r.RdvDate.Day
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(x => x.RdvStatut == 1),
                    ArrivedAppointments = g.Count(x => x.RdvArrivee.Value.Date == DateTime.Today),
                    CancelledAppointments = g.Count(x => x.Localisation == 5),
                    AverageDuration = g.Average(x => (double?)x.RdvDuree) ?? 0
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
                .ToListAsync();

            return results.Select(r => new AppointmentStats
            {
                Year = (int)r.Year,
                Month = (int)r.Month,
                Day = (int)r.Day,
                Period = $"{r.Year:0000}-{r.Month:00}-{r.Day:00}",
                TotalAppointments = r.TotalAppointments,
                CompletedAppointments = r.CompletedAppointments,
                 ArrivedAppointments = r.ArrivedAppointments,
                CancelledAppointments = r.CancelledAppointments,
                AverageDuration = r.AverageDuration,
                GroupingType = GroupingType.Day
            }).ToList();
        }

        private async Task<List<AppointmentStats>> GroupByMonth(IQueryable<RendezVous> query)
        {
            var results = await query
                .GroupBy(r => new
                {
                    Year = r.RdvDate.Year,
                    Month = r.RdvDate.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(x => x.RdvStatut == 1),
                    ArrivedAppointments = g.Count(x => x.RdvArrivee.Value.Date == DateTime.Today),
                    CancelledAppointments = g.Count(x => x.Localisation == 5),
                    AverageDuration = g.Average(x => (double?)x.RdvDuree) ?? 0
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return results.Select(r => new AppointmentStats
            {
                Year = (int)r.Year,
                Month = (int)r.Month,
                Period = $"{r.Year:0000}-{r.Month:00}",
                TotalAppointments = r.TotalAppointments,
                CompletedAppointments = r.CompletedAppointments,
                 ArrivedAppointments = r.ArrivedAppointments,
                CancelledAppointments = r.CancelledAppointments,
                AverageDuration = r.AverageDuration,
                GroupingType = GroupingType.Month
            }).ToList();
        }

        private async Task<List<AppointmentStats>> GroupByYear(IQueryable<RendezVous> query)
        {
            var results = await query
                .GroupBy(r => r.RdvDate.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalAppointments = g.Count(),
                    ArrivedAppointments = g.Count(x => x.RdvArrivee.Value.Date == DateTime.Today),
                    CompletedAppointments = g.Count(x => x.RdvStatut == 1),
                    CancelledAppointments = g.Count(x => x.Localisation == 5),
                    AverageDuration = g.Average(x => (double?)x.RdvDuree) ?? 0
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            return results.Select(r => new AppointmentStats
            {
                Year = (int)r.Year,
                Period = r.Year.ToString(),
                TotalAppointments = r.TotalAppointments,
                CompletedAppointments = r.CompletedAppointments,
                CancelledAppointments = r.CancelledAppointments,
                 ArrivedAppointments = r.ArrivedAppointments,
                AverageDuration = r.AverageDuration,
                GroupingType = GroupingType.Year
            }).ToList();
        }
        private List<AppointmentStats> FillGaps(
    List<AppointmentStats> stats,
    DateTime startDate,
    DateTime endDate,
    GroupingType groupingType)
        {
            var filled = new List<AppointmentStats>();

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

                        filled.Add(found ?? new AppointmentStats
                        {
                            Year = dt.Year,
                            Month = dt.Month,
                            Day = dt.Day,
                            Hour = dt.Hour,
                            Period = $"{dt:yyyy-MM-dd HH}:00",
                            TotalAppointments = 0,
                            CompletedAppointments = 0,
                            CancelledAppointments = 0,
                            AverageDuration = 0,
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

                        filled.Add(found ?? new AppointmentStats
                        {
                            Year = dt.Year,
                            Month = dt.Month,
                            Day = dt.Day,
                            Period = $"{dt:yyyy-MM-dd}",
                            TotalAppointments = 0,
                            CompletedAppointments = 0,
                            CancelledAppointments = 0,
                            AverageDuration = 0,
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

                        filled.Add(found ?? new AppointmentStats
                        {
                            Year = dt.Year,
                            Month = dt.Month,
                            Period = $"{dt:yyyy-MM}",
                            TotalAppointments = 0,
                            CompletedAppointments = 0,
                            CancelledAppointments = 0,
                            AverageDuration = 0,
                            GroupingType = GroupingType.Month
                        });
                    }
                    break;

                case GroupingType.Year:
                    for (int year = startDate.Year; year <= endDate.Year; year++)
                    {
                        var found = stats.FirstOrDefault(s => s.Year == year);

                        filled.Add(found ?? new AppointmentStats
                        {
                            Year = year,
                            Period = year.ToString(),
                            TotalAppointments = 0,
                            CompletedAppointments = 0,
                            CancelledAppointments = 0,
                            AverageDuration = 0,
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

    }
}
