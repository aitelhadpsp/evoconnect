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
    public class KpiController : ControllerBase
    {
        private readonly IKpiConfigRepository _repository;
        private readonly IAppointmentsDA _appointmentsDA;

        public KpiController(IKpiConfigRepository repository,IAppointmentsDA appointmentsDA)
        {
            _repository = repository;
            _appointmentsDA = appointmentsDA;
        }

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
        public async Task<ActionResult<KpiBatchSaveResponse>> GetStats()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(1).AddSeconds(-1);
            var appStats =äwait _appointmentsDA.GetAppointmentStatsAsync(start, end);
            return null;

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
}