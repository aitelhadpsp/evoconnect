using EvoConnect.Server.DTOs;

namespace EvoConnect.Server.Repository.Interfaces
{
    public interface IActesRepository
    {
        /// <summary>
        /// Gets all actes with pagination, including code secu information
        /// </summary>
        Task<PaginatedResult<DentalisActeDto>> GetAllActesAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Gets all actes for a specific patient without pagination
        /// </summary>
        Task<List<ActePatientDto>> GetActesByPatientIdAsync(int patientId);

        /// <summary>
        /// Gets a single acte by ID with code secu information
        /// </summary>
        Task<DentalisActeDto?> GetActeByIdAsync(int acteId);

        /// <summary>
        /// Gets a single acte patient by ID
        /// </summary>
        Task<ActePatientDto?> GetActePatientByIdAsync(int apPk);
    }
}