using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EvoConnect.Server.Controllers
{
	public class PatientController(IPartnerDA _partnerDA, IPatientsDA _patientsDA) : Controller
	{
		[HttpGet("patients/{id}")]
		public async Task<IActionResult> GetPatient(int id)
		{
			var data = await _partnerDA.GetPatient(id);
			return Ok(data);
		}

		[HttpGet("patients")]
		public async Task<IActionResult> GetPatients([FromQuery] string? search)
		{
			var data = await _partnerDA.GetPatients(search);
			return Ok(data);
		}

		[HttpGet("patients/{id}/images")]
		public async Task<IActionResult> GetPatientImages(int id, [FromQuery] List<string>? labels)
		{
			var ids = labels != null ? string.Join(",", labels) : null;
			var data = await _partnerDA.GetPatientImages(id, ids);
			return Ok(data);
		}

		[HttpGet("patients/{id}/images/comp")]
		public async Task<IActionResult> GetCompImages(int id, [FromQuery] List<string> labels)
		{
			var ids = string.Join(",", labels);
			var data = await _partnerDA.GetPatientImagesWithLabels(id, ids);
			return Ok(data);
		}

		// New methods from IPatientsDA interface

		/// <summary>
		/// Get paginated patients with filters
		/// </summary>
		[HttpPost("patients/paginated")]
		public async Task<IActionResult> GetPaginatedPatients()
		{
			try
			{
				// Read the raw request body
				using var reader = new StreamReader(Request.Body);
				var body = await reader.ReadToEndAsync();

				Console.WriteLine($"Raw request body: {body}");

				if (string.IsNullOrEmpty(body))
				{
					return BadRequest("Empty request body");
				}

				// Try to deserialize manually
				var request = JsonSerializer.Deserialize<PatientFilterRequest>(body, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});

				if (request == null)
				{
					return BadRequest("Failed to deserialize request");
				}

				Console.WriteLine($"Deserialized filters: {request.Filters?.Name}");
				Console.WriteLine($"Deserialized pagination: Page {request.Pagination?.PageNumber}, Size {request.Pagination?.PageSize}");

				var data = await _patientsDA.GetPaginatedPatientsAsync(request);
				return Ok(data);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return BadRequest($"Error: {ex.Message}");
			}
		}

		/// <summary>
		/// Get patient count with filters
		/// </summary>
		[HttpPost("patients/count")]
		public async Task<IActionResult> GetPatientCount([FromBody] PatientFilters filters)
		{
			var count = await _patientsDA.GetPatientCountAsync(filters);
			return Ok(new { count });
		}

		/// <summary>
		/// Get patient by ID with full details
		/// </summary>
		[HttpGet("patients/details/{id}")]
		public async Task<IActionResult> GetPatientById(int id)
		{
			var patient = await _patientsDA.GetPatientByIdAsync(id);

			if (patient == null)
			{
				return NotFound();
			}

			return Ok(patient);
		}

		/// <summary>
		/// Search patients with pagination
		/// </summary>
		[HttpGet("patients/search")]
		public async Task<IActionResult> SearchPatients(
			[FromQuery] string searchText,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 20)
		{
			var data = await _patientsDA.SearchPatientsAsync(searchText, pageNumber, pageSize);
			return Ok(data);
		}

		[HttpGet("stats/creation")]
		public async Task<IActionResult> GetPatientCreationStatistics(
		[FromQuery] DateTime fromDate,
		[FromQuery] DateTime toDate)
		{
			// Validate date range
			if (fromDate > toDate)
			{
				return BadRequest("From date cannot be greater than to date");
			}

			// Validate date range is not too large (prevent performance issues)
			var daysDiff = (toDate - fromDate).Days;
			if (daysDiff > 10 * 365) // 10 years
			{
				return BadRequest("Date range cannot exceed 10 years");
			}

		/* 	// Ensure dates are not in the future
			var today = DateTime.Now.Date;
			if (fromDate > today || toDate > today)
			{
				return BadRequest("Date range cannot be in the future");
			} */

			try
			{
				var stats = await _patientsDA.GetPatientCreationStatisticsAsync(fromDate, toDate);

				// Calculate summary information
				var totalPatients = stats.Sum(s => s.Count);
				var avgPatientsPerPeriod = stats.Any() ? stats.Average(s => s.Count) : 0;
				var maxPatientsInPeriod = stats.Any() ? stats.Max(s => s.Count) : 0;
				var periodsWithPatients = stats.Count(s => s.Count > 0);

				return Ok(new
				{
					fromDate,
					toDate,
					totalPatients,
					avgPatientsPerPeriod = Math.Round(avgPatientsPerPeriod, 2),
					maxPatientsInPeriod,
					periodsWithPatients,
					totalPeriods = stats.Count,
					groupingLevel = stats.FirstOrDefault()?.GroupingLevel.ToString() ?? "Day",
					data = stats
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error retrieving patient creation statistics: {ex.Message}");
			}
		}

		/// <summary>
		/// Get patient engagement statistics
		/// Returns counts and percentages for:
		/// - Patients with no appointments
		/// - Patients with appointments but never showed up
		/// - Patients with no realized treatments
		/// - Patients with at least one realized treatment
		/// </summary>
		[HttpGet("stats/engagement")]
		public async Task<IActionResult> GetPatientEngagementStatistics()
		{
			try
			{
				var stats = await _patientsDA.GetPatientEngagementStatisticsAsync();
				return Ok(stats);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error retrieving patient engagement statistics: {ex.Message}");
			}
		}

		/// <summary>
		/// Get detailed patient engagement statistics with breakdowns by appointment status
		/// Includes additional metrics like no-shows, cancellations, and planned treatments
		/// </summary>
		[HttpGet("stats/engagement/detailed")]
		public async Task<IActionResult> GetDetailedPatientEngagementStatistics()
		{
			try
			{
				var stats = await _patientsDA.GetDetailedPatientEngagementStatisticsAsync();
				return Ok(stats);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error retrieving detailed patient engagement statistics: {ex.Message}");
			}
		}
	}
}