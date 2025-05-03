// File: backend/NotificationService/Services/DigestBackgroundService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Models;

namespace NotificationService.Services;

public class DigestBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DigestBackgroundService> _logger;
    private readonly DigestOptions _options;

    public DigestBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<DigestOptions> options,
        ILogger<DigestBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Digest Background Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = CalculateNextRunTime(now);
            var delay = nextRun - now;

            _logger.LogInformation("Next digest will run at {NextRun}, in {DelayHours} hours and {DelayMinutes} minutes",
                nextRun, delay.Hours, delay.Minutes);

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await GenerateDigestAsync(stoppingToken);
            }
        }

        _logger.LogInformation("Digest Background Service is stopping");
    }

    private DateTime CalculateNextRunTime(DateTime now)
    {
        var nextRun = new DateTime(now.Year, now.Month, now.Day, _options.DigestHour, 0, 0);
        
        if (now.Hour >= _options.DigestHour)
        {
            // If we've already passed the hour for today, schedule for tomorrow
            nextRun = nextRun.AddDays(1);
        }
        
        return nextRun;
    }

    private async Task GenerateDigestAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Generating daily digest");
            
            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var digestGenerator = scope.ServiceProvider.GetRequiredService<IDigestGenerator>();
            
            var digest = await digestGenerator.GenerateDailyDigestAsync();
            
            _logger.LogInformation("Daily digest generation complete. Digest ID: {DigestId}", digest.DigestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during digest generation");
        }
    }
}
