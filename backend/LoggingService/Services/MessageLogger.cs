// File: backend/LoggingService/Services/MessageLogger.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LoggingService.Data;
using LoggingService.Models;
using Common.Models;

namespace LoggingService.Services;

public class MessageLogger : IMessageLogger
{
    private readonly LoggingDbContext _context;

    public MessageLogger(LoggingDbContext context)
    {
        _context = context;
    }

    public async Task LogMessageAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMessageStatusAsync(string messageId, string status)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            message.Status = status;
            message.LastUpdatedAt = DateTime.UtcNow;
            
            // Update retention date based on status
            if (status == MessageStatus.Sent)
            {
                message.RetentionDate = DateTime.UtcNow.AddMonths(3);
            }
            else if (status == MessageStatus.Failed)
            {
                message.RetentionDate = DateTime.MaxValue; // Indefinite retention
            }
            
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Message> GetMessageAsync(string messageId)
    {
        return await _context.Messages.FindAsync(messageId);
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(MessageFilterModel filter)
    {
        var query = _context.Messages.AsQueryable();

        if (!string.IsNullOrEmpty(filter.PatientId))
            query = query.Where(m => m.PatientId == filter.PatientId);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(m => m.Status == filter.Status);

        if (!string.IsNullOrEmpty(filter.ClinicId))
            query = query.Where(m => m.ClinicId == filter.ClinicId);

        if (filter.FromDate.HasValue)
            query = query.Where(m => m.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(m => m.CreatedAt <= filter.ToDate.Value);

        return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
    }

    public async Task PurgeExpiredMessagesAsync()
    {
        var now = DateTime.UtcNow;
        var expiredMessages = await _context.Messages
            .Where(m => m.RetentionDate < now)
            .ToListAsync();

        _context.Messages.RemoveRange(expiredMessages);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetFailedMessageCountAsync()
    {
        return await _context.Messages
            .CountAsync(m => m.Status == MessageStatus.Failed);
    }

    public async Task<int> GetOutstandingMessageCountAsync()
    {
        var yesterday = DateTime.UtcNow.AddDays(-1);
        return await _context.Messages
            .CountAsync(m => m.Status == MessageStatus.Pending && m.CreatedAt < yesterday);
    }
}
