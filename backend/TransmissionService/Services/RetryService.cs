// File: backend/TransmissionService/Services/RetryService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Common.Services;
using TransmissionService.Models;

namespace TransmissionService.Services;

public class RetryService : BackgroundService
{
    private readonly IServiceBusClient _serviceBusClient;
    private readonly IHl7Transmitter _hl7Transmitter;
    private readonly ILogger<RetryService> _logger;
    private readonly TransmissionOptions _options;
    private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(5);

    public RetryService(
        IServiceBusClient serviceBusClient,
        IHl7Transmitter hl7Transmitter,
        IOptions<TransmissionOptions> options,
        ILogger<RetryService> logger)
    {
        _serviceBusClient = serviceBusClient;
        _hl7Transmitter = hl7Transmitter;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Retry Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Process messages in the retry queue
                await ProcessRetryQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during retry processing");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Retry Service is stopping");
    }

    private async Task ProcessRetryQueueAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Processing retry queue");

        // Get messages from the retry queue
        var retryMessages = await _serviceBusClient.ReceiveMessagesFromRetryQueueAsync(10, stoppingToken);

        foreach (var message in retryMessages)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogInformation("Retrying message {MessageId}, attempt {AttemptCount}", 
                    message.MessageId, message.RetryCount);

                // Implement exponential backoff if configured
                if (_options.UseBackoffStrategy && message.RetryCount > 1)
                {
                    int delayMs = _options.RetryDelayMilliseconds * (int)Math.Pow(2, message.RetryCount - 1);
                    await Task.Delay(delayMs, stoppingToken);
                }

                // Attempt to retransmit
                var result = await _hl7Transmitter.TransmitMessage(message.MessageId, message.Hl7Content);

                if (result.Success)
                {
                    _logger.LogInformation("Retry successful for message {MessageId}", message.MessageId);
                    
                    // Update status in the logging service
                    await _serviceBusClient.PublishStatusUpdateAsync(message.MessageId, "Sent");
                    
                    // Complete the message (remove from queue)
                    await _serviceBusClient.CompleteRetryMessageAsync(message.LockToken);
                }
                else
                {
                    // Check if max retry attempts reached
                    if (message.RetryCount >= _options.MaxRetryAttempts)
                    {
                        _logger.LogWarning("Max retry attempts reached for message {MessageId}, marking as failed", 
                            message.MessageId);
                        
                        // Update status to failed in the logging service
                        await _serviceBusClient.PublishStatusUpdateAsync(message.MessageId, "Failed");
                        
                        // Complete the message (remove from queue)
                        await _serviceBusClient.CompleteRetryMessageAsync(message.LockToken);
                    }
                    else
                    {
                        // Increment retry count and reschedule
                        _logger.LogInformation("Retry failed for message {MessageId}, rescheduling", message.MessageId);
                        
                        // Abandon the message (it will be returned to the queue)
                        await _serviceBusClient.AbandonRetryMessageAsync(message.LockToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing retry for message {MessageId}", message.MessageId);
                
                // Abandon the message
                await _serviceBusClient.AbandonRetryMessageAsync(message.LockToken);
            }
        }
    }
}
