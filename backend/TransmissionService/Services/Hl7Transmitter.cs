// File: backend/TransmissionService/Services/Hl7Transmitter.cs
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransmissionService.Models;

namespace TransmissionService.Services;

public class Hl7Transmitter : IHl7Transmitter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Hl7Transmitter> _logger;
    private readonly TransmissionOptions _options;

    public Hl7Transmitter(
        HttpClient httpClient,
        IOptions<TransmissionOptions> options,
        ILogger<Hl7Transmitter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<TransmissionResult> TransmitMessage(string messageId, string hl7Message)
    {
        var result = new TransmissionResult
        {
            MessageId = messageId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Measure response time
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Prepare the content
            var content = new StringContent(hl7Message, Encoding.UTF8, "application/hl7-v2");
            
            // Add necessary headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("MessageId", messageId);
            
            // Send the request to the Ontario PCR endpoint
            var response = await _httpClient.PostAsync(_options.EndpointUrl, content);

            stopwatch.Stop();
            result.ResponseTime = (int)stopwatch.ElapsedMilliseconds;
            
            if (response.IsSuccessStatusCode)
            {
                var ackResponse = await response.Content.ReadAsStringAsync();
                
                // Parse the ACK response
                var ackType = ParseAcknowledgementType(ackResponse);
                
                result.Success = ackType == "AA"; // Application Accept
                result.AcknowledgmentType = ackType;
                
                if (ackType == "AR" || ackType == "AE") // Application Reject or Error
                {
                    result.ErrorDetails = ParseErrorDetails(ackResponse);
                }
            }
            else
            {
                result.Success = false;
                result.AcknowledgmentType = "RE"; // Reject Error
                result.ErrorDetails = $"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transmit HL7 message {MessageId}", messageId);
            
            result.Success = false;
            result.AcknowledgmentType = "CE"; // Connection Error
            result.ErrorDetails = $"Connection Error: {ex.Message}";
        }

        return result;
    }

    private string ParseAcknowledgementType(string ackResponse)
    {
        try
        {
            // Simple parsing logic for HL7 ACK message
            // Format: MSH|...|ACK^messageType|...|AA/AE/AR
            var lines = ackResponse.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.StartsWith("MSA|"))
                {
                    var fields = line.Split('|');
                    if (fields.Length >= 2)
                    {
                        return fields[1]; // The acknowledgment code
                    }
                }
            }
            
            return "UE"; // Unknown Error
        }
        catch
        {
            return "PE"; // Parsing Error
        }
    }

    private string ParseErrorDetails(string ackResponse)
    {
        try
        {
            // Look for ERR segments
            var lines = ackResponse.Split('\n');
            var errors = new StringBuilder();
            
            foreach (var line in lines)
            {
                if (line.StartsWith("ERR|"))
                {
                    var fields = line.Split('|');
                    if (fields.Length >= 4)
                    {
                        errors.AppendLine(fields[3]); // Error message
                    }
                }
            }
            
            return errors.ToString().Trim();
        }
        catch
        {
            return "Unable to parse error details";
        }
    }
}
