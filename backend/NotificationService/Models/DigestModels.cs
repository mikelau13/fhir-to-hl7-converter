// File: backend/NotificationService/Models/DigestModels.cs
using System;
using System.Collections.Generic;

namespace NotificationService.Models;

public class DigestModel
{
    public string DigestId { get; set; }
    public DateTime GeneratedDate { get; set; }
    public int ErrorCount { get; set; }
    public int OutstandingCount { get; set; }
    public bool SentStatus { get; set; }
    public List<ConnectivityError> ConnectivityErrors { get; set; }
    public List<NackResponse> NackResponses { get; set; }
    public List<OutstandingMessage> OutstandingMessages { get; set; }
}

public class ConnectivityError
{
    public string ErrorId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
}

public class NackResponse
{
    public string MessageId { get; set; }
    public string PatientId { get; set; }
    public DateTime Timestamp { get; set; }
    public string AcknowledgmentType { get; set; }
    public string ErrorDetails { get; set; }
}

public class OutstandingMessage
{
    public string MessageId { get; set; }
    public string PatientId { get; set; }
    public string ClinicId { get; set; }
    public string ClinicName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
    public int ResendCount { get; set; }
}
