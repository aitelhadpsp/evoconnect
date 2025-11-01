using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EvoConnect.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController(IAppointmentsDA _appointmentsDA) : ControllerBase
    {
        #region Appointment Data Endpoints

        /// <summary>
        /// Get appointments by date range
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAppointments(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var appointments = await _appointmentsDA.GetAppointmentsByDateRangeAsync(fromDate, toDate);
            return Ok(appointments);
        }

        /// <summary>
        /// Get upcoming appointments (next 7 days by default)
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingAppointments([FromQuery] int days = 7)
        {
            if (days < 1 || days > 365)
            {
                return BadRequest("Days must be between 1 and 365");
            }

            var appointments = await _appointmentsDA.GetUpcomingAppointmentsAsync(days);
            return Ok(appointments);
        }

        #endregion

        #region Statistics Endpoints

        /// <summary>
        /// Get monthly appointment statistics for charts
        /// </summary>
        [HttpGet("stats/monthly")]
        public async Task<IActionResult> GetMonthlyStats([FromQuery] int year = 0)
        {
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            if (year < 2000 || year > DateTime.Now.Year + 5)
            {
                return BadRequest("Invalid year");
            }

            var stats = await _appointmentsDA.GetMonthlyAppointmentStatsAsync(year);
            return Ok(new { year, data = stats });
        }

        /// <summary>
        /// Get appointment statistics by procedure type (acte)
        /// </summary>
        [HttpGet("stats/by-acte")]
        public async Task<IActionResult> GetStatsByActe(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var stats = await _appointmentsDA.GetAppointmentsByActeAsync(fromDate, toDate);

            // Calculate percentages
            var total = stats.Sum(s => s.TotalAppointments);
            foreach (var stat in stats)
            {
                stat.Percentage = total > 0 ? (double)stat.TotalAppointments / total * 100 : 0;
            }

            return Ok(new
            {
                totalAppointments = total,
                fromDate,
                toDate,
                data = stats
            });
        }

        /// <summary>
        /// Get appointment statistics by chair/room (fauteuil)
        /// </summary>
        [HttpGet("stats/by-fauteuil")]
        public async Task<IActionResult> GetStatsByFauteuil(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var stats = await _appointmentsDA.GetAppointmentsByFauteuilAsync(fromDate, toDate);

            // Calculate percentages
            var total = stats.Sum(s => s.TotalAppointments);
            foreach (var stat in stats)
            {
                stat.Percentage = total > 0 ? (double)stat.TotalAppointments / total * 100 : 0;
            }

            return Ok(new
            {
                totalAppointments = total,
                fromDate,
                toDate,
                data = stats
            });
        }

        /// <summary>
        /// Get daily appointment statistics
        /// </summary>
        [HttpGet("stats/daily")]
        public async Task<IActionResult> GetDailyStats(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var daysDiff = (toDate - fromDate).Days;
            if (daysDiff > 365)
            {
                return BadRequest("Date range cannot exceed 365 days");
            }

            var stats = await _appointmentsDA.GetDailyAppointmentStatsAsync(fromDate, toDate);
            return Ok(new { fromDate, toDate, data = stats });
        }
        [HttpGet("/stats/dynamic")]
        public async Task<IActionResult> GetAppointmentStatsAsync(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {

            var stats = await _appointmentsDA.GetAppointmentStatsAsync(fromDate, toDate);
            return Ok(stats);
        }

        /// <summary>
        /// Get comprehensive summary statistics
        /// </summary>
        [HttpGet("stats/summary")]
        public async Task<IActionResult> GetSummaryStats(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var stats = await _appointmentsDA.GetAppointmentSummaryStatsAsync(fromDate, toDate);
            return Ok(new { fromDate, toDate, stats });
        }

        #endregion

        #region Analytics Endpoints

        /// <summary>
        /// Get complete analytics dashboard data
        /// </summary>
        [HttpPost("analytics/dashboard")]
        public async Task<IActionResult> GetDashboardAnalytics([FromBody] AnalyticsRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            if (request.FromDate > request.ToDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }



            var summaryStats = await _appointmentsDA.GetAppointmentSummaryStatsAsync(request.FromDate, request.ToDate);
            var acteStats = await _appointmentsDA.GetAppointmentsByActeAsync(request.FromDate, request.ToDate);
            var fauteuilStats = await _appointmentsDA.GetAppointmentsByFauteuilAsync(request.FromDate, request.ToDate);
            var dailyStats = await _appointmentsDA.GetDailyAppointmentStatsAsync(request.FromDate, request.ToDate);

            // Calculate percentages for pie charts
            var totalActes = acteStats.Sum(s => s.TotalAppointments);
            foreach (var stat in acteStats)
            {
                stat.Percentage = totalActes > 0 ? (double)stat.TotalAppointments / totalActes * 100 : 0;
            }

            var totalFauteuils = fauteuilStats.Sum(s => s.TotalAppointments);
            foreach (var stat in fauteuilStats)
            {
                stat.Percentage = totalFauteuils > 0 ? (double)stat.TotalAppointments / totalFauteuils * 100 : 0;
            }

            return Ok(new DashboardAnalyticsResponse
            {
                Period = new { FromDate = request.FromDate, ToDate = request.ToDate },
                Summary = summaryStats,
                ActeStats = acteStats,
                FauteuilStats = fauteuilStats,
                DailyStats = dailyStats
            });
        }

        /// <summary>
        /// Get year-over-year comparison
        /// </summary>
        [HttpGet("analytics/year-comparison")]
        public async Task<IActionResult> GetYearComparison(
            [FromQuery] int currentYear = 0,
            [FromQuery] int previousYear = 0)
        {
            if (currentYear == 0) currentYear = DateTime.Now.Year;
            if (previousYear == 0) previousYear = currentYear - 1;

            var currentYearStats = await _appointmentsDA.GetMonthlyAppointmentStatsAsync(currentYear);
            var previousYearStats = await _appointmentsDA.GetMonthlyAppointmentStatsAsync(previousYear);

            return Ok(new
            {
                currentYear,
                previousYear,
                currentYearData = currentYearStats,
                previousYearData = previousYearStats
            });
        }

        #endregion

        #region Reference Data Endpoints

        /// <summary>
        /// Get all procedure types (actes)
        /// </summary>
        [HttpGet("actes")]
        public async Task<IActionResult> GetAllActes()
        {
            var actes = await _appointmentsDA.GetAllActesAsync();
            return Ok(actes);
        }

        /// <summary>
        /// Get all chairs/rooms (fauteuils)
        /// </summary>
        [HttpGet("fauteuils")]
        public async Task<IActionResult> GetAllFauteuils()
        {
            var fauteuils = await _appointmentsDA.GetAllFauteuilsAsync();
            return Ok(fauteuils);
        }

        #endregion

        #region Utility Endpoints

        /// <summary>
        /// Get available date ranges with data
        /// </summary>
        [HttpGet("date-ranges")]
        public async Task<IActionResult> GetAvailableDateRanges()
        {
            try
            {
                // This would typically query the earliest and latest appointment dates
                // For now, return common ranges
                var currentYear = DateTime.Now.Year;
                var ranges = new List<object>
                {
                    new {
                        Label = "This Month",
                        FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                        ToDate = DateTime.Now
                    },
                    new {
                        Label = "Last Month",
                        FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1),
                        ToDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1)
                    },
                    new {
                        Label = "This Year",
                        FromDate = new DateTime(currentYear, 1, 1),
                        ToDate = DateTime.Now
                    },
                    new {
                        Label = "Last Year",
                        FromDate = new DateTime(currentYear - 1, 1, 1),
                        ToDate = new DateTime(currentYear - 1, 12, 31)
                    }
                };

                return Ok(ranges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving date ranges: {ex.Message}");
            }
        }

        /// <summary>
        /// Export appointments data
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportAppointments([FromBody] ExportRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            if (request.FromDate > request.ToDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var appointments = await _appointmentsDA.GetAppointmentsByDateRangeAsync(request.FromDate, request.ToDate);

            // For now, return JSON. In a real implementation, you might generate CSV/Excel
            return Ok(new
            {
                exportDate = DateTime.Now,
                period = new { request.FromDate, request.ToDate },
                format = request.Format,
                totalRecords = appointments.Count,
                data = appointments
            });
        }

        #endregion

    }


    public class AnalyticsRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IncludeDetails { get; set; } = true;
    }

    public class ExportRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Format { get; set; } = "json"; // json, csv, excel
        public List<string> Fields { get; set; } = new();
    }

    public class DashboardAnalyticsResponse
    {
        public object Period { get; set; }
        public AppointmentSummaryStats Summary { get; set; }
        public List<ActeStats> ActeStats { get; set; }
        public List<FauteuilStats> FauteuilStats { get; set; }
        public List<DailyAppointmentStats> DailyStats { get; set; }
    }

}