using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Server.Data;
using EvoConnect.Server.Models;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvoConnect.Server.Repository
{
     public class KpiConfigRepository : IKpiConfigRepository
    {
        private readonly ClinicDbContext _context;

        public KpiConfigRepository(ClinicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets all currently enabled KPI configurations
        /// </summary>
        public async Task<List<KpiConfig>> GetAllEnabledAsync()
        {
            return await _context.KpiConfigs
                .Where(k => k.IsEnabled == 1)
                .OrderBy(k => k.DisplayOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Saves a batch of new KPI configurations, disables all previous ones
        /// </summary>
     public async Task<List<KpiConfig>> SaveBatchAsync(List<KpiConfig> newConfigs)
{
    if (newConfigs == null || !newConfigs.Any())
        throw new ArgumentException("At least one KPI configuration is required", nameof(newConfigs));
    
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        var now = DateTime.Now;
        var updatedConfigs = new List<KpiConfig>();
        
        foreach (var newConfig in newConfigs)
        {
            // Find existing config by code
            var existing = await _context.KpiConfigs
                .FirstOrDefaultAsync(k => k.KpiCode == newConfig.KpiCode.ToUpper());
            
            if (existing != null)
            {
                // Update existing
                existing.KpiName = newConfig.KpiName;
                existing.KpiIcon = newConfig.KpiIcon;
                existing.TargetValue = newConfig.TargetValue;
                existing.Unit = newConfig.Unit;
                existing.WarningThreshold = newConfig.WarningThreshold;
                existing.CriticalThreshold = newConfig.CriticalThreshold;
                existing.Description = newConfig.Description;
                existing.DisplayOrder = newConfig.DisplayOrder;
                existing.IsEnabled = 1;
                existing.UpdatedAt = now;
                
                updatedConfigs.Add(existing);
            }
            else
            {
                // Create new
                newConfig.KpiCode = newConfig.KpiCode.ToUpper();
                newConfig.IsEnabled = 1;
                newConfig.CreatedAt = now;
                newConfig.UpdatedAt = now;
                
                await _context.KpiConfigs.AddAsync(newConfig);
                updatedConfigs.Add(newConfig);
            }
        }
        
        // Disable any configs not in the new list
        var newCodes = newConfigs.Select(c => c.KpiCode.ToUpper()).ToList();
        var toDisable = await _context.KpiConfigs
            .Where(k => k.IsEnabled == 1 && !newCodes.Contains(k.KpiCode))
            .ToListAsync();
        
        foreach (var config in toDisable)
        {
            config.IsEnabled = 0;
            config.UpdatedAt = now;
        }
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return updatedConfigs;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
    }
}