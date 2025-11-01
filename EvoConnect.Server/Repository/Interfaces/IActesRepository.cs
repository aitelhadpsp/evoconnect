using EvoConnect.Server.DTOs;

namespace EvoConnect.Server.Repository.Interfaces
{
    public interface IActesRepository
    {
        Task<PaginatedResult<DentalisActeDto>> GetAllActesAsync(int pageNumber, int pageSize);

        Task<List<ActePatientDto>> GetActesByPatientIdAsync(int patientId);

        Task<DentalisActeDto?> GetActeByIdAsync(int acteId);       
        Task<int> GetTodayRealisedActesCount();       
        Task<float> GetTodayRealisedActesEncaiss();       
        Task<ActePatientDto?> GetActePatientByIdAsync(int apPk);
    }
}