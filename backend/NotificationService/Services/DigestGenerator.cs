// File: backend/NotificationService/Services/DigestGenerator.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Models;

namespace NotificationService.Services;

public interface IDigestGenerator
{
    Task<DigestModel> GenerateDailyDigestAsync();
}

public class DigestGenerator : IDigestGenerator
{
    private readonly HttpClient _httpClient;
    private readonly IEmailSender _emailSender;
    private readonly DigestOptions _options;
    private readonly ILogger<DigestGenerator> _logger;

    public DigestGenerator(
        HttpClient httpClient,
        IEmailSender emailSender,
        IOptions<DigestOptions> options,
        ILogger<DigestGenerator> logger)
    {
        _httpClient = httpClient;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DigestModel> GenerateDailyDigestAsync()
    {
        var digest = new DigestModel
        {
            DigestId = Guid.NewGuid().ToString(),
            GeneratedDate = DateTime.UtcNow,
            ConnectivityErrors = new List<ConnectivityError>(),
            NackResponses = new List<NackResponse>(),
            OutstandingMessages = new List<OutstandingMessage>()
        };

        try
        {
            // Fetch connectivity errors from transmission service
            var connectivityErrors = await GetConnectivityErrorsAsync();
            digest.ConnectivityErrors.AddRange(connectivityErrors);
            
            // Fetch NACK responses from transmission service
            var nackResponses = await GetNackResponsesAsync();
            digest.NackResponses.AddRange(nackResponses);
            
            // Fetch outstanding messages from logging service
            var outstandingMessages = await GetOutstandingMessagesAsync();
            digest.OutstandingMessages.AddRange(outstandingMessages);
            
            // Calculate totals
            digest.ErrorCount = digest.ConnectivityErrors.Count + digest.NackResponses.Count;
            digest.OutstandingCount = digest.OutstandingMessages.Count;
            
            // Send email if there are issues to report
            if (digest.ErrorCount > 0 || digest.OutstandingCount > 0)
            {
                await _emailSender.SendDigestEmailAsync(digest, _options.Recipients);
                digest.SentStatus = true;
                _logger.LogInformation("Sent digest email {DigestId} with {ErrorCount} errors and {OutstandingCount} outstanding messages",
                    digest.DigestId, digest.ErrorCount, digest.OutstandingCount);
            }
            else
            {
                _logger.LogInformation("No issues to report in digest {DigestId}, email not sent", digest.DigestId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate daily digest");
            throw;
        }

        return digest;
    }

    private async Task<IEnumerable<ConnectivityError>> GetConnectivityErrorsAsync()
    {
        try
        {
            var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            var response = await _httpClient.GetFromJsonAsync<List<ConnectivityError>>(
                $"{_options.TransmissionServiceUrl}/api/transmission/errors?fromDate={yesterday}");
            
            return response ?? new List<ConnectivityError>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch connectivity errors");
            return new List<ConnectivityError>();
        }
    }

    private async Task<IEnumerable<NackResponse>> GetNackResponsesAsync()
    {
        try
        {
            var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            var response = await _httpClient.GetFromJsonAsync<List<NackResponse>>(
                $"{_options.TransmissionServiceUrl}/api/transmission/nacks?fromDate={yesterday}");
            
            return response ?? new List<NackResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch NACK responses");
            return new List<NackResponse>();
        }
    }

    private async Task<IEnumerable<OutstandingMessage>> GetOutstandingMessagesAsync()
    {
        try
        {
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd");
            var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            
            var response = await _httpClient.GetFromJsonAsync<List<OutstandingMessage>>(
                $"{_options.LoggingServiceUrl}/api/logging?status=Pending&fromDate={twoDaysAgo}&toDate={yesterday}");
            
            return response ?? new List<OutstandingMessage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch outstanding messages");
            return new List<OutstandingMessage>();
        }
    }
}
