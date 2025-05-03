// File: backend/ResendApiService/Models/ResendApiModels.cs
using System;

namespace ResendApiService.Models;

public class ResendApiOptions
{
    public string LoggingServiceUrl { get; set; }
    public string TransmissionServiceUrl { get; set; }
}

public class MessageFilterModel
{
    public string PatientId { get; set; }
    public string Status { get; set; }
    public string ClinicId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class MessageModel
{
    public string MessageId { get; set; }
    public string PatientId { get; set; }
    public string ClinicId { get; set; }
    public string ClinicName { get; set; }
    public string OriginalFhirContent { get; set; }
    public string ConvertedHl7Content { get; set; }
    public string Hl7Content => ConvertedHl7Content; // For compatibility with frontend
    public string MessageType { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public int ResendCount { get; set; }
}

public class ClinicModel
{
    public string ClinicId { get; set; }
    public string ClinicName { get; set; }
    public bool IsActive { get; set; }
    public string Settings { get; set; }
}

public class ResendRequestModel
{
    public bool EditBeforeResend { get; set; }
    public string UpdatedContent { get; set; }
}

public class BatchResendRequestModel
{
    public string[] MessageIds { get; set; }
    public bool EditBeforeResend { get; set; }
}

public class MessageContentUpdateModel
{
    public string UpdatedContent { get; set; }
}

public class ClinicStatusUpdateModel
{
    public bool IsActive { get; set; }
}
