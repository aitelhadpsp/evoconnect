using EvoConnect.Server.Data;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;

namespace EvoConnect.Server.Services 
{
    public class VipStatsRefreshService
    {
        private readonly ClinicDbContext _context;
        private readonly ILogger<VipStatsRefreshService> _logger;

        public VipStatsRefreshService(
            ClinicDbContext context,
            ILogger<VipStatsRefreshService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Refreshes the VIP patient stats materialized view
        /// </summary>
        public async Task<RefreshResult> RefreshVipStatsAsync()
        {
            try
            {
                var startTime = DateTime.Now;

                // Execute the stored procedure
                await _context.Database.ExecuteSqlRawAsync("EXECUTE PROCEDURE SP_REFRESH_VIP_STATS");

                // Get count of records in the materialized view
                var countSql = "SELECT CAST(COUNT(*) AS INTEGER) AS TOTAL_COUNT FROM VIP_PATIENT_STATS";
                var countResult = await _context.Database
                    .SqlQueryRaw<CountResult>(countSql)
                    .FirstOrDefaultAsync();

                var recordCount = countResult?.TOTAL_COUNT ?? 0;
                var duration = (DateTime.Now - startTime).TotalMilliseconds;

                _logger.LogInformation(
                    "VIP stats refreshed successfully. Records: {Count}, Duration: {Duration}ms",
                    recordCount, duration);

                return new RefreshResult
                {
                    RecordsCount = recordCount,
                    Status = "SUCCESS",
                    Message = $"VIP stats refreshed successfully. {recordCount} records processed in {duration:F0}ms.",
                    Duration = (int)duration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing VIP stats");
                
                return new RefreshResult
                {
                    RecordsCount = 0,
                    Status = "ERROR",
                    Message = $"Error refreshing VIP stats: {ex.Message}",
                    Duration = 0
                };
            }
        }

        /// <summary>
        /// Gets basic statistics about VIP patients
        /// </summary>
        public async Task<VipStatsDto> GetVipStatisticsAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        COUNT(*) AS TOTAL_COUNT,
                        SUM(CASE WHEN STATUT = 'Actif' THEN 1 ELSE 0 END) AS ACTIVE_COUNT,
                        SUM(CASE WHEN STATUT = 'À risque' THEN 1 ELSE 0 END) AS AT_RISK_COUNT,
                        COALESCE(SUM(CA_ANNUEL), 0) AS TOTAL_CA_ANNUEL,
                        COALESCE(SUM(CA_GLOBAL), 0) AS TOTAL_CA_GLOBAL,
                        MAX(LAST_UPDATED) AS LAST_REFRESH
                    FROM VIP_PATIENT_STATS";

                var result = await _context.Database
                    .SqlQueryRaw<VipStatsDto>(sql)
                    .FirstOrDefaultAsync();

                return result ?? new VipStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VIP statistics");
                return new VipStatsDto();
            }
        }

        /// <summary>
        /// Checks if the materialized view needs refresh (older than 1 hour)
        /// </summary>
        public async Task<bool> NeedsRefreshAsync()
        {
            try
            {
                var sql = "SELECT MAX(LAST_UPDATED) AS LAST_UPDATED FROM VIP_PATIENT_STATS";
                var result = await _context.Database
                    .SqlQueryRaw<LastUpdateDto>(sql)
                    .FirstOrDefaultAsync();

                if (result?.LAST_UPDATED == null)
                    return true;

                var timeSinceUpdate = DateTime.Now - result.LAST_UPDATED.Value;
                return timeSinceUpdate.TotalHours >= 1;
            }
            catch
            {
                return true;
            }
        }

  

        /// <summary>
   }

    // DTOs
    public class RefreshResult
    {
        public int RecordsCount { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int Duration { get; set; }
    }

    public class CountResult
    {
        public int TOTAL_COUNT { get; set; }
    }

    public class VipStatsDto
    {
        public int TOTAL_COUNT { get; set; }
        public int ACTIVE_COUNT { get; set; }
        public int AT_RISK_COUNT { get; set; }
        public decimal TOTAL_CA_ANNUEL { get; set; }
        public decimal TOTAL_CA_GLOBAL { get; set; }
        public DateTime? LAST_REFRESH { get; set; }
    }

    public class LastUpdateDto
    {
        public DateTime? LAST_UPDATED { get; set; }
    }
}