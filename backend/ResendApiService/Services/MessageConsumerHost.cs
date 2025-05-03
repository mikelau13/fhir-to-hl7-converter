// File: backend/TransmissionService/Services/MessageConsumerHost.cs
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Common.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransmissionService.Services;

namespace TransmissionService.Services;

public class MessageConsumerHost : BackgroundService
{
    private readonly IServiceBusClient _serviceBusClient;
    private readonly IHl7Transmitter _hl7Transmitter;
    private readonly ILogger<MessageConsumerHost> _logger;
    private readonly ServiceBusProcessor _processor;

    public MessageConsumerHost(
        Azure.Messaging.ServiceBus.ServiceBusClient serviceBusClient,
        IOptions<ServiceBusConnectionOptions> serviceBusOptions,
        IHl7Transmitter hl7Transmitter,
        ILogger<MessageConsumerHost> logger)
    {
        _hl7Transmitter = hl7Transmitter;
        _logger = logger;
        
        // Create processor for the HL7 queue
        _processor = serviceBusClient.CreateProcessor(
            serviceBusOptions.Value.Hl7QueueName,
            new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 10,
                AutoCompleteMessages = false
            });
            
        // Add message handler
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting HL7 message consumer");
        
        // Start processing
        await _processor.StartProcessingAsync(stoppingToken);
        
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
            _logger.LogInformation("Stopping HL7 message consumer");
            await _processor.StopProcessingAsync(CancellationToken.None);
        }
    }
    
    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        string messageBody = args.Message.Body.ToString();
        string messageId = args.Message.MessageId;
        
        _logger.LogInformation("Received HL7 message {MessageId} for transmission", messageId);
        
        try
        {
            // Deserialize the message
            var hl7Message = JsonSerializer.Deserialize<Hl7Message>(messageBody);
            
            if (hl7Message == null)
            {
                _logger.LogError("Failed to deserialize HL7 message {MessageId}", messageId);
                await args.CompleteMessageAsync(args.Message);
                return;
            }
            
            // Transmit the message
            var result = await _hl7Transmitter.TransmitMessage(hl7Message.MessageId, hl7Message.Hl7Content);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully transmitted HL7 message {MessageId}", messageId);
                
                // Update status
                await _serviceBusClient.PublishStatusUpdateAsync(hl7Message.MessageId, "Sent");
                
                // Complete the message
                await args.CompleteMessageAsync(args.Message);
            }
            else
            {
                _logger.LogWarning("Failed to transmit HL7 message {MessageId}: {ErrorDetails}", 
                    messageId, result.ErrorDetails);
                
                // Send to retry queue
                await _serviceBusClient.SendToRetryQueueAsync(
                    hl7Message.MessageId, 
                    hl7Message.Hl7Content, 
                    1);
                
                // Complete the message (it will be handled by the retry service)
                await args.CompleteMessageAsync(args.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing HL7 message {MessageId}", messageId);
            
            // Abandon the message to retry processing
            await args.AbandonMessageAsync(args.Message);
        }
    }
    
    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in HL7 message consumer: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }
}
