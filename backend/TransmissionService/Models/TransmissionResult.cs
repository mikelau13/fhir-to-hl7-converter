// File: backend/TransmissionService/Models/TransmissionResult.cs
using System;

namespace TransmissionService.Models;

public class TransmissionResult
{
    public string MessageId { get; set; }
    public bool Success { get; set; }
    public string AcknowledgmentType { get; set; }
    public DateTime Timestamp { get; set; }
    public string ErrorDetails { get; set; }
    public int ResponseTime { get; set; } // in milliseconds
}
