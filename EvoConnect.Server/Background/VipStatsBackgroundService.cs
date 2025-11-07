using EvoConnect.Server.Services;

namespace  EvoConnect.Server.Background;
public class VipStatsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VipStatsBackgroundService> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromHours(1);

    public VipStatsBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<VipStatsBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VIP Stats Background Service started");

        // Initial refresh on startup
        await RefreshStatsAsync(stoppingToken);

        using var timer = new PeriodicTimer(_refreshInterval);

        while (!stoppingToken.IsCancellationRequested && 
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RefreshStatsAsync(stoppingToken);
        }
    }

    private async Task RefreshStatsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var refreshService = scope.ServiceProvider
                .GetRequiredService<VipStatsRefreshService>();

           await refreshService.RefreshVipStatsAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in VIP stats background refresh");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("VIP Stats Background Service stopping");
        await base.StopAsync(cancellationToken);
    }
}