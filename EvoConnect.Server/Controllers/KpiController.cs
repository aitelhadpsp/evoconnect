using EvoConnect.Server.Models;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace EvoConnect.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KpiController(
        IPaymentDA _paymentDA,
        IPatientsDA _patientsDA,
        IActesRepository _actesRepository,
        IKpiConfigRepository _repository,
        IAppointmentsDA _appointmentsDA) : ControllerBase
    {
        /// <summary>
        /// Get all currently enabled KPI configurations
        /// </summary>
        [HttpGet("enabled")]
        public async Task<ActionResult<List<KpiConfig>>> GetEnabled()
        {
            try
            {
                var configs = await _repository.GetAllEnabledAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving KPI configurations", error = ex.Message });
            }
        }

        /// <summary>
        /// Get KPI statistics for today
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<KpiStatsResponse>> GetStats()
        {
            try
            {
                var start = DateTime.Today;
                var end = DateTime.Today.AddDays(1).AddSeconds(-1);
                var appointments = await _appointmentsDA.GetAppointmentStatsAsync(start, end);
                var realisedActesCount = await _actesRepository.GetTodayRealisedActesCount();
                var encaiss = await _actesRepository.GetTodayRealisedActesEncaiss();
                var totalPayment = await _paymentDA.GetTotalPaymentsToday();
                var patients = (await _patientsDA.GetPatientCreationStatisticsAsync(start.AddMonths(-1), start)).Sum(p => p.Count);
                var lostPatients = await _patientsDA.GetLostPatients();
                var activePatients = await _patientsDA.GetTotalActivePatients();

                return Ok(new KpiStatsResponse
                {
                    TotalAppointments = appointments.Sum(a => a.TotalAppointments),
                    CancelledAppointments = appointments.Sum(a => a.CancelledAppointments),
                    ArrivedAppointments = appointments.Sum(a => a.ArrivedAppointments),
                    RealisedActesCount = realisedActesCount,
                    Encaiss = encaiss,
                    NewPatientsCount = patients,
                    TotalPayment = totalPayment,
                    FetchedAt = DateTime.UtcNow,
                    LostPatients = lostPatients,
                    ActivePatients = activePatients
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving KPI statistics", error = ex.Message });
            }
        }
        [HttpGet("stats/productivity")]
        public async Task<ActionResult> GetProductivityStats()
        {
            try
            {
                var start = DateTime.Today.AddDays(-30);
                var end = DateTime.Today.AddDays(1).AddSeconds(-1);
                var lastStart = start.AddMonths(-1);
                var lastEnd = end.AddMonths(-1);
                var LastMonthPayments = await _paymentDA.GetDailyRevenueAsync(start, end);
                var paymentsToday = await _paymentDA.GetPaymentCountAsync(new PaymentFilters
                {
                    DateFrom = DateTime.Today,
                    DateTo = end
                });
                var LastMonthappointments = (await _appointmentsDA.GetAppointmentStatsAsync(start, end)).Sum(e=> e.TotalAppointments);
                var BeforeLastMonthPayments = (await _paymentDA.GetDailyRevenueAsync(lastStart, lastEnd));
                var BeforeLastMonthappointments = (await _appointmentsDA.GetAppointmentStatsAsync(lastStart, lastEnd)).Sum(e=> e.TotalAppointments);


                return Ok(new
                {
                    LastMonthPayments,
                    BeforeLastMonthPayments,
                    LastMonthappointments,
                    BeforeLastMonthappointments,
                    paymentsToday
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving KPI statistics", error = ex.Message });
            }
        }


        /// <summary>
        /// Save a batch of KPI configurations (disables old, enables new)
        /// </summary>
        [HttpPost("batch")]
        public async Task<ActionResult<KpiBatchSaveResponse>> SaveBatch([FromBody] KpiBatchSaveRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = errors
                });
            }

            if (request?.Configs == null || request.Configs.Count == 0)
            {
                return BadRequest(new { message = "At least one KPI configuration is required" });
            }

            try
            {
                // Map DTOs to entities
                var configs = request.Configs.Select(dto => new KpiConfig
                {
                    KpiCode = dto.KpiCode?.ToUpper() ?? throw new ArgumentException("KpiCode is required"),
                    KpiName = dto.KpiName ?? throw new ArgumentException("KpiName is required"),
                    KpiIcon = dto.KpiIcon,
                    TargetValue = dto.TargetValue,
                    Unit = dto.Unit,
                    WarningThreshold = dto.WarningThreshold,
                    CriticalThreshold = dto.CriticalThreshold,
                    Description = dto.Description,
                    DisplayOrder = dto.DisplayOrder
                }).ToList();

                var savedConfigs = await _repository.SaveBatchAsync(configs);

                return Ok(new KpiBatchSaveResponse
                {
                    Success = true,
                    SavedCount = savedConfigs.Count,
                    Message = $"{savedConfigs.Count} KPI configurations saved successfully",
                    Configs = savedConfigs
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving KPI configurations", error = ex.Message });
            }
        }
    }

    public class KpiBatchSaveRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("configs")]
        [Required(ErrorMessage = "Configs list is required")]
        public List<KpiConfigDto> Configs { get; set; } = new();
    }

    public class KpiConfigDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("kpiCode")]
        [Required(ErrorMessage = "KPI Code is required")]
        public string KpiCode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("kpiName")]
        [Required(ErrorMessage = "KPI Name is required")]
        public string KpiName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("kpiIcon")]
        public string? KpiIcon { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("targetValue")]
        public decimal? TargetValue { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("warningThreshold")]
        public decimal? WarningThreshold { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("criticalThreshold")]
        public decimal? CriticalThreshold { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string? Description { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("displayOrder")]
        public int? DisplayOrder { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isEnabled")]
        public short? IsEnabled { get; set; }
    }

    public class KpiBatchSaveResponse
    {
        public bool Success { get; set; }
        public int SavedCount { get; set; }
        public string Message { get; set; }
        public List<KpiConfig> Configs { get; set; }
    }

    public class KpiStatsResponse
    {
        public int TotalAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int ArrivedAppointments { get; set; }
        public int RealisedActesCount { get; set; }
        public int NewPatientsCount { get; set; }
        public float Encaiss { get; set; }
        public float TotalPayment { get; set; }
        public int LostPatients { get; set; }
        public int ActivePatients { get; set; }
        public DateTime FetchedAt { get; set; }
    }
}