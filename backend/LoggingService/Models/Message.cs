// File: backend/LoggingService/Models/Message.cs
using System;

namespace LoggingService.Models;

public class Message
{
    public string MessageId { get; set; }
    public string PatientId { get; set; }
    public string ClinicId { get; set; }
    public string OriginalFhirContent { get; set; }
    public string ConvertedHl7Content { get; set; }
    public string MessageType { get; set; }  // A28, A31, A40
    public string Status { get; set; }  // Pending, Sent, Failed
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public int ResendCount { get; set; }
    public DateTime RetentionDate { get; set; }
}