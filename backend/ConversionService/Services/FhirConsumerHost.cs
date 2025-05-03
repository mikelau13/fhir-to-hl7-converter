// File: backend/ConversionService/Services/FhirConsumerHost.cs
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Common.Models;
using Common.Services;
using ConversionService.Converters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace ConversionService.Services;

public class FhirConsumerHost : BackgroundService
{
    private readonly AdtConverter _adtConverter;
    private readonly IServiceBusClient _serviceBusClient;
    private readonly ILogger<FhirConsumerHost> _logger;
    private readonly ServiceBusProcessor _processor;
    private readonly FhirJsonParser _fhirParser;

    public FhirConsumerHost(
        Azure.Messaging.ServiceBus.ServiceBusClient serviceBusClient,
        IOptions<ServiceBusConnectionOptions> serviceBusOptions,
        AdtConverter adtConverter,
        IServiceBusClient messagingClient,
        ILogger<FhirConsumerHost> logger)
    {
        _adtConverter = adtConverter;
        _serviceBusClient = messagingClient;
        _logger = logger;
        _fhirParser = new FhirJsonParser();
        
        // Create processor for the FHIR queue
        _processor = serviceBusClient.CreateProcessor(
            serviceBusOptions.Value.FhirQueueName,
            new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 10,
                AutoCompleteMessages = false
            });
            
        // Add message handler
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting FHIR message consumer");
        
        // Start processing
        await _processor.StartProcessingAsync(stoppingToken);
        
        try
        {
            await System.Threading.Tasks.Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
            _logger.LogInformation("Stopping FHIR message consumer");
            await _processor.StopProcessingAsync(CancellationToken.None);
        }
    }
    
    private async System.Threading.Tasks.Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        string messageBody = args.Message.Body.ToString();
        string messageId = args.Message.MessageId;
        
        _logger.LogInformation("Received FHIR message {MessageId} for conversion", messageId);
        
        try
        {
            // Deserialize the message
            var fhirMessage = JsonSerializer.Deserialize<FhirMessage>(messageBody);
            
            if (fhirMessage == null)
            {
                _logger.LogError("Failed to deserialize FHIR message {MessageId}", messageId);
                await args.CompleteMessageAsync(args.Message);
                return;
            }
            
            // Parse the FHIR resource
            var resource = _fhirParser.Parse<Patient>(fhirMessage.FhirContent);
            
            // Convert to HL7 based on message type
            string hl7Content = await ConvertFhirToHl7(resource, fhirMessage.MessageType);
            
            if (string.IsNullOrEmpty(hl7Content))
            {
                _logger.LogError("Failed to convert FHIR message {MessageId} to HL7", messageId);
                await args.AbandonMessageAsync(args.Message);
                return;
            }
            
            // Publish the converted HL7 message
            await _serviceBusClient.PublishHl7MessageAsync(fhirMessage.MessageId, hl7Content);
            
            _logger.LogInformation("Successfully converted FHIR message {MessageId} to HL7", messageId);
            
            // Complete the message
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing FHIR message {MessageId}", messageId);
            
            // Abandon the message to retry processing
            await args.AbandonMessageAsync(args.Message);
        }
    }
    
    private async Task<string> ConvertFhirToHl7(Patient patient, string messageType)
    {
        switch (messageType)
        {
            case MessageType.A28:
                return await _adtConverter.ConvertA28(patient);
                
            case MessageType.A31:
                return await _adtConverter.ConvertA31(patient);
                
            case MessageType.A40:
                return await _adtConverter.ConvertA40(patient);
                
            default:
                _logger.LogWarning("Unsupported message type: {MessageType}", messageType);
                return null;
        }
    }
    
    private System.Threading.Tasks.Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in FHIR message consumer: {ErrorSource}", args.ErrorSource);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
