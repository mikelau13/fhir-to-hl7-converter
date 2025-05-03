// File: backend/LoggingService/Controllers/LoggingController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LoggingService.Models;
using LoggingService.Services;

namespace LoggingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoggingController : ControllerBase
{
    private readonly IMessageLogger _messageLogger;

    public LoggingController(IMessageLogger messageLogger)
    {
        _messageLogger = messageLogger;
    }

    [HttpPost]
    public async Task<IActionResult> LogMessage([FromBody] Message message)
    {
        await _messageLogger.LogMessageAsync(message);
        return Ok();
    }

    [HttpPut("{messageId}/status")]
    public async Task<IActionResult> UpdateStatus(string messageId, [FromBody] string status)
    {
        await _messageLogger.UpdateMessageStatusAsync(messageId, status);
        return Ok();
    }

    [HttpGet("{messageId}")]
    public async Task<IActionResult> GetMessage(string messageId)
    {
        var message = await _messageLogger.GetMessageAsync(messageId);
        if (message == null)
            return NotFound();
        
        return Ok(message);
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] MessageFilterModel filter)
    {
        var messages = await _messageLogger.GetMessagesAsync(filter);
        return Ok(messages);
    }

    [HttpGet("stats/failed-count")]
    public async Task<IActionResult> GetFailedCount()
    {
        var count = await _messageLogger.GetFailedMessageCountAsync();
        return Ok(count);
    }

    [HttpGet("stats/outstanding-count")]
    public async Task<IActionResult> GetOutstandingCount()
    {
        var count = await _messageLogger.GetOutstandingMessageCountAsync();
        return Ok(count);
    }
}
