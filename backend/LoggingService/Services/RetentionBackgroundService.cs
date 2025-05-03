// File: backend/LoggingService/Services/RetentionBackgroundService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoggingService.Services;

public class RetentionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RetentionBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(24);

    public RetentionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RetentionBackgroundService> logger)
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
                using var scope = _serviceProvider.CreateScope();
                var messageLogger = scope.ServiceProvider.GetRequiredService<IMessageLogger>();
                
                _logger.LogInformation("Running message retention cleanup...");
                await messageLogger.PurgeExpiredMessagesAsync();
                _logger.LogInformation("Message retention cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during message retention cleanup");
            }

            await Task.Delay(_period, stoppingToken);
        }
    }
}
