// File: backend/LoggingService/Services/IMessageLogger.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using LoggingService.Models;

namespace LoggingService.Services;

public interface IMessageLogger
{
    Task LogMessageAsync(Message message);
    Task UpdateMessageStatusAsync(string messageId, string status);
    Task<Message> GetMessageAsync(string messageId);
    Task<IEnumerable<Message>> GetMessagesAsync(MessageFilterModel filter);
    Task PurgeExpiredMessagesAsync();
    Task<int> GetFailedMessageCountAsync();
    Task<int> GetOutstandingMessageCountAsync();
}

public class MessageFilterModel
{
    public string PatientId { get; set; }
    public string Status { get; set; }
    public string ClinicId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
