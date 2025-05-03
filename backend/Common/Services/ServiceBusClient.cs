// File: backend/Common/Services/ServiceBusClient.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Common.Services;

public interface IServiceBusClient
{
    Task PublishFhirMessageAsync(string messageId, string fhirContent, string clinicId, string patientId, string messageType);
    Task PublishHl7MessageAsync(string messageId, string hl7Content);
    Task PublishStatusUpdateAsync(string messageId, string status);
    Task SendToRetryQueueAsync(string messageId, string hl7Content, int retryCount);
    Task<IEnumerable<RetryMessage>> ReceiveMessagesFromRetryQueueAsync(int maxMessages, CancellationToken cancellationToken);
    Task CompleteRetryMessageAsync(string lockToken);
    Task AbandonRetryMessageAsync(string lockToken);
}

public class ServiceBusClient : IServiceBusClient, IAsyncDisposable
{
    private readonly ServiceBusConnectionOptions _options;
    private readonly ILogger<ServiceBusClient> _logger;
    private readonly Azure.Messaging.ServiceBus.ServiceBusClient _client;
    private readonly ServiceBusSender _fhirSender;
    private readonly ServiceBusSender _hl7Sender;
    private readonly ServiceBusSender _statusSender;
    private readonly ServiceBusSender _retrySender;
    private readonly ServiceBusReceiver _retryReceiver;
    private readonly ConcurrentDictionary<string, ServiceBusReceivedMessage> _receivedMessages = new();

    public ServiceBusClient(
        IOptions<ServiceBusConnectionOptions> options,
        ILogger<ServiceBusClient> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Create the client
        _client = new Azure.Messaging.ServiceBus.ServiceBusClient(_options.ConnectionString);

        // Create senders
        _fhirSender = _client.CreateSender(_options.FhirQueueName);
        _hl7Sender = _client.CreateSender(_options.Hl7QueueName);
        _statusSender = _client.CreateSender(_options.StatusQueueName);
        _retrySender = _client.CreateSender(_options.RetryQueueName);

        // Create receiver
        _retryReceiver = _client.CreateReceiver(_options.RetryQueueName);
    }

    public async Task PublishFhirMessageAsync(string messageId, string fhirContent, string clinicId, string patientId, string messageType)
    {
        try
        {
            var message = new FhirMessage
            {
                MessageId = messageId,
                FhirContent = fhirContent,
                ClinicId = clinicId,
                PatientId = patientId,
                MessageType = messageType,
                Timestamp = DateTime.UtcNow
            };

            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage);
            serviceBusMessage.MessageId = messageId;
            serviceBusMessage.ApplicationProperties.Add("MessageType", messageType);

            await _fhirSender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation("Published FHIR message {MessageId} to queue", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish FHIR message {MessageId} to queue", messageId);
            throw;
        }
    }

    public async Task PublishHl7MessageAsync(string messageId, string hl7Content)
    {
        try
        {
            var message = new Hl7Message
            {
                MessageId = messageId,
                Hl7Content = hl7Content,
                Timestamp = DateTime.UtcNow
            };

            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage);
            serviceBusMessage.MessageId = messageId;

            await _hl7Sender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation("Published HL7 message {MessageId} to queue", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish HL7 message {MessageId} to queue", messageId);
            throw;
        }
    }

    public async Task PublishStatusUpdateAsync(string messageId, string status)
    {
        try
        {
            var message = new StatusUpdateMessage
            {
                MessageId = messageId,
                Status = status,
                Timestamp = DateTime.UtcNow
            };

            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage);
            serviceBusMessage.MessageId = Guid.NewGuid().ToString();
            serviceBusMessage.ApplicationProperties.Add("MessageId", messageId);
            serviceBusMessage.ApplicationProperties.Add("Status", status);

            await _statusSender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation("Published status update for message {MessageId}: {Status}", messageId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish status update for message {MessageId}", messageId);
            throw;
        }
    }

    public async Task SendToRetryQueueAsync(string messageId, string hl7Content, int retryCount)
    {
        try
        {
            var message = new RetryMessage
            {
                MessageId = messageId,
                Hl7Content = hl7Content,
                RetryCount = retryCount,
                Timestamp = DateTime.UtcNow
            };

            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage);
            serviceBusMessage.MessageId = messageId;
            serviceBusMessage.ApplicationProperties.Add("RetryCount", retryCount);

            // Set a scheduled time with backoff
            if (retryCount > 1)
            {
                int delaySeconds = (int)Math.Pow(2, retryCount - 1) * 10; // Exponential backoff
                serviceBusMessage.ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
            }

            await _retrySender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation("Sent message {MessageId} to retry queue, attempt {RetryCount}", messageId, retryCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message {MessageId} to retry queue", messageId);
            throw;
        }
    }

    public async Task<IEnumerable<RetryMessage>> ReceiveMessagesFromRetryQueueAsync(int maxMessages, CancellationToken cancellationToken)
    {
        try
        {
            var receivedMessages = await _retryReceiver.ReceiveMessagesAsync(maxMessages, TimeSpan.FromSeconds(5), cancellationToken);
            var retryMessages = new List<RetryMessage>();

            foreach (var receivedMessage in receivedMessages)
            {
                try
                {
                    var messageBody = receivedMessage.Body.ToString();
                    var retryMessage = JsonSerializer.Deserialize<RetryMessage>(messageBody);
                    
                    if (retryMessage != null)
                    {
                        // Add lock token for later processing
                        retryMessage.LockToken = receivedMessage.LockToken;
                        retryMessages.Add(retryMessage);
                        
                        // Store the received message for later use
                        _receivedMessages[receivedMessage.LockToken] = receivedMessage;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize retry message");
                    await _retryReceiver.AbandonMessageAsync(receivedMessage);
                }
            }

            return retryMessages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive messages from retry queue");
            return Array.Empty<RetryMessage>();
        }
    }

    public async Task CompleteRetryMessageAsync(string lockToken)
    {
        try
        {
            if (_receivedMessages.TryGetValue(lockToken, out var receivedMessage))
            {
                await _retryReceiver.CompleteMessageAsync(receivedMessage);
                _receivedMessages.TryRemove(lockToken, out _);
            }
            else
            {
                _logger.LogWarning("Could not find received message with lock token: {LockToken}", lockToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete retry message");
            throw;
        }
    }

    public async Task AbandonRetryMessageAsync(string lockToken)
    {
        try
        {
            if (_receivedMessages.TryGetValue(lockToken, out var receivedMessage))
            {
                await _retryReceiver.AbandonMessageAsync(receivedMessage);
                _receivedMessages.TryRemove(lockToken, out _);
            }
            else
            {
                _logger.LogWarning("Could not find received message with lock token: {LockToken}", lockToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to abandon retry message");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _fhirSender.DisposeAsync();
        await _hl7Sender.DisposeAsync();
        await _statusSender.DisposeAsync();
        await _retrySender.DisposeAsync();
        await _retryReceiver.DisposeAsync();
        await _client.DisposeAsync();
    }
}

public class ServiceBusConnectionOptions
{
    public string ConnectionString { get; set; }
    public string FhirQueueName { get; set; } = "fhir-queue";
    public string Hl7QueueName { get; set; } = "hl7-queue";
    public string StatusQueueName { get; set; } = "status-queue";
    public string RetryQueueName { get; set; } = "retry-queue";
}

public class FhirMessage
{
    public string MessageId { get; set; }
    public string FhirContent { get; set; }
    public string ClinicId { get; set; }
    public string PatientId { get; set; }
    public string MessageType { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Hl7Message
{
    public string MessageId { get; set; }
    public string Hl7Content { get; set; }
    public DateTime Timestamp { get; set; }
}

public class StatusUpdateMessage
{
    public string MessageId { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
}

public class RetryMessage
{
    public string MessageId { get; set; }
    public string Hl7Content { get; set; }
    public int RetryCount { get; set; }
    public DateTime Timestamp { get; set; }
    public string LockToken { get; set; } // For internal use
}
