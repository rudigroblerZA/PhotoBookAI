using PhotoBook.RenderService.Data;
using Microsoft.EntityFrameworkCore;

namespace PhotoBook.RenderService.Services;

public class RenderJobCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RenderJobCleanupService> _logger;

    public RenderJobCleanupService(IServiceProvider serviceProvider, ILogger<RenderJobCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldJobsAsync();
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cleanup service");
            }
        }
    }

    private async Task CleanupOldJobsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RenderDbContext>();

        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        var oldJobs = await context.RenderJobs
            .Where(j => j.CompletedAt < cutoffDate)
            .ToListAsync();

        if (oldJobs.Count > 0)
        {
            context.RenderJobs.RemoveRange(oldJobs);
            await context.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} old render jobs", oldJobs.Count);
        }
    }
}
