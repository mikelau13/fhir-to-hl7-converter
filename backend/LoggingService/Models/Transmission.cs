// File: backend/LoggingService/Models/Transmission.cs
using System;

namespace LoggingService.Models;

public class Transmission
{
    public string TransmissionId { get; set; }
    public string MessageId { get; set; }
    public DateTime AttemptedAt { get; set; }
    public string Status { get; set; }
    public string AcknowledgmentType { get; set; }
    public string ErrorDetails { get; set; }
    public int ResponseTime { get; set; }
}
