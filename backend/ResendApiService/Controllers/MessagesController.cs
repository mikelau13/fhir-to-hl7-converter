// File: backend/ResendApiService/Controllers/MessagesController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Common.Models;
using Common.Services;
using ResendApiService.Models;

namespace ResendApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IServiceBusClient _serviceBusClient;
    private readonly ILogger<MessagesController> _logger;
    private readonly IOptions<ResendApiOptions> _options;

    public MessagesController(
        HttpClient httpClient,
        IServiceBusClient serviceBusClient,
        ILogger<MessagesController> logger,
        IOptions<ResendApiOptions> options)
    {
        _httpClient = httpClient;
        _serviceBusClient = serviceBusClient;
        _logger = logger;
        _options = options;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] MessageFilterModel filter)
    {
        try
        {
            // Build query string from filter
            var queryParams = new StringBuilder();
            
            if (!string.IsNullOrEmpty(filter.PatientId))
                queryParams.Append($"&patientId={filter.PatientId}");
                
            if (!string.IsNullOrEmpty(filter.Status))
                queryParams.Append($"&status={filter.Status}");
                
            if (!string.IsNullOrEmpty(filter.ClinicId))
                queryParams.Append($"&clinicId={filter.ClinicId}");
                
            if (filter.FromDate.HasValue)
                queryParams.Append($"&fromDate={filter.FromDate:yyyy-MM-dd}");
                
            if (filter.ToDate.HasValue)
                queryParams.Append($"&toDate={filter.ToDate:yyyy-MM-dd}");
            
            var query = queryParams.Length > 0 
                ? $"?{queryParams.ToString().Substring(1)}" 
                : "";
                
            // Fetch messages from logging service
            var messages = await _httpClient.GetFromJsonAsync<List<MessageModel>>(
                $"{_options.Value.LoggingServiceUrl}/api/logging{query}");
                
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch messages");
            return StatusCode(500, "Failed to fetch messages");
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMessage(string id)
    {
        try
        {
            // Fetch message from logging service
            var message = await _httpClient.GetFromJsonAsync<MessageModel>(
                $"{_options.Value.LoggingServiceUrl}/api/logging/{id}");
                
            if (message == null)
                return NotFound();
                
            return Ok(message);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch message {MessageId}", id);
            return StatusCode(500, "Failed to fetch message");
        }
    }
    
    [HttpPost("{id}/resend")]
    public async Task<IActionResult> ResendMessage(string id, [FromBody] ResendRequestModel request)
    {
        try
        {
            // Fetch message from logging service
            var message = await _httpClient.GetFromJsonAsync<MessageModel>(
                $"{_options.Value.LoggingServiceUrl}/api/logging/{id}");
                
            if (message == null)
                return NotFound();
            
            // If user wants to edit the content, update it first
            if (request.EditBeforeResend && !string.IsNullOrEmpty(request.UpdatedContent))
            {
                await _httpClient.PutAsJsonAsync(
                    $"{_options.Value.LoggingServiceUrl}/api/logging/{id}/content",
                    new { content = request.UpdatedContent });
                    
                // Use the updated content for transmission
                message.ConvertedHl7Content = request.UpdatedContent;
            }
            
            // Send to service bus for processing
            await _serviceBusClient.SendToRetryQueueAsync(
                id, 
                message.Hl7Content, 
                message.ResendCount + 1);
            
            // Update status in the logging service to Pending
            await _httpClient.PutAsJsonAsync(
                $"{_options.Value.LoggingServiceUrl}/api/logging/{id}/status",
                MessageStatus.Pending);
            
            return Ok(new { success = true, message = "Message queued for resend" });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend message {MessageId}", id);
            return StatusCode(500, "Failed to resend message");
        }
    }
    
    [HttpPost("batch-resend")]
    public async Task<IActionResult> BatchResend([FromBody] BatchResendRequestModel request)
    {
        if (request.MessageIds == null || request.MessageIds.Length == 0)
            return BadRequest("No message IDs provided");
            
        var results = new List<ResendResult>();
        
        foreach (var id in request.MessageIds)
        {
            try
            {
                // Fetch message from logging service
                var message = await _httpClient.GetFromJsonAsync<MessageModel>(
                    $"{_options.Value.LoggingServiceUrl}/api/logging/{id}");
                    
                if (message == null)
                {
                    results.Add(new ResendResult 
                    { 
                        MessageId = id, 
                        Success = false, 
                        Message = "Message not found" 
                    });
                    continue;
                }
                
                // Send to service bus for processing
                await _serviceBusClient.SendToRetryQueueAsync(
                    id, 
                    message.Hl7Content, 
                    message.ResendCount + 1);
                
                // Update status in the logging service to Pending
                await _httpClient.PutAsJsonAsync(
                    $"{_options.Value.LoggingServiceUrl}/api/logging/{id}/status",
                    MessageStatus.Pending);
                
                results.Add(new ResendResult 
                { 
                    MessageId = id, 
                    Success = true, 
                    Message = "Message queued for resend" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend message {MessageId}", id);
                
                results.Add(new ResendResult 
                { 
                    MessageId = id, 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                });
            }
        }
        
        return Ok(new { results });
    }
    
    [HttpPut("{id}/content")]
    public async Task<IActionResult> UpdateMessageContent(string id, [FromBody] MessageContentUpdateModel update)
    {
        try
        {
            // Update content in the logging service
            var response = await _httpClient.PutAsJsonAsync(
                $"{_options.Value.LoggingServiceUrl}/api/logging/{id}/content",
                new { content = update.UpdatedContent });
                
            response.EnsureSuccessStatusCode();
            
            return Ok(new { success = true });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update message content for {MessageId}", id);
            return StatusCode(500, "Failed to update message content");
        }
    }
}

public class ResendResult
{
    public string MessageId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}
