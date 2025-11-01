using Microsoft.AspNetCore.Mvc;
using EvoConnect.Server.DTOs;
using EvoConnect.Server.Repository.Interfaces;

namespace EvoConnect.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActesController(
        IActesRepository actesRepository,
        ILogger<ActesController> logger) : ControllerBase
    {
        private readonly IActesRepository _actesRepository = actesRepository;
        private readonly ILogger<ActesController> _logger = logger;

        /// <summary>
        /// Get all actes with pagination (read-only)
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 50, max: 100)</param>
        /// <returns>Paginated list of actes</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<DentalisActeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<DentalisActeDto>>> GetAllActes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest("Page number must be greater than 0");
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("Page size must be between 1 and 100");
                }

                var result = await _actesRepository.GetAllActesAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving actes with pagination");
                return StatusCode(500, "An error occurred while retrieving actes");
            }
        }

        /// <summary>
        /// Get acte by ID (read-only)
        /// </summary>
        /// <param name="id">Acte ID</param>
        /// <returns>Acte details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DentalisActeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DentalisActeDto>> GetActeById(int id)
        {
            try
            {
                var acte = await _actesRepository.GetActeByIdAsync(id);
                
                if (acte == null)
                {
                    return NotFound($"Acte with ID {id} not found");
                }

                return Ok(acte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving acte with ID {ActeId}", id);
                return StatusCode(500, "An error occurred while retrieving the acte");
            }
        }

        /// <summary>
        /// Get all actes for a specific patient without pagination (read-only)
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>List of patient actes</returns>
        [HttpGet("patient/{patientId}")]
        [ProducesResponseType(typeof(List<ActePatientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ActePatientDto>>> GetActesByPatient(int patientId)
        {
            try
            {
                if (patientId <= 0)
                {
                    return BadRequest("Invalid patient ID");
                }

                var actes = await _actesRepository.GetActesByPatientIdAsync(patientId);
                return Ok(actes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving actes for patient {PatientId}", patientId);
                return StatusCode(500, "An error occurred while retrieving patient actes");
            }
        }

        /// <summary>
        /// Get acte patient by ID (read-only)
        /// </summary>
        /// <param name="id">Acte Patient ID (AP_PK)</param>
        /// <returns>Acte patient details</returns>
        [HttpGet("patient/detail/{id}")]
        [ProducesResponseType(typeof(ActePatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ActePatientDto>> GetActePatientById(int id)
        {
            try
            {
                var actePatient = await _actesRepository.GetActePatientByIdAsync(id);
                
                if (actePatient == null)
                {
                    return NotFound($"Acte patient with ID {id} not found");
                }

                return Ok(actePatient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving acte patient with ID {ActePatientId}", id);
                return StatusCode(500, "An error occurred while retrieving the acte patient");
            }
        }
    }
}