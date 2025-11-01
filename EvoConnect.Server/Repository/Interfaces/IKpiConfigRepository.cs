using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Server.Models;

namespace EvoConnect.Server.Repository.Interfaces
{
 public interface IKpiConfigRepository
    {
     
        Task<List<KpiConfig>> GetAllEnabledAsync();
        Task<List<KpiConfig>> SaveBatchAsync(List<KpiConfig> newConfigs);
    }
}