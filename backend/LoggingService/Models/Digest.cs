// File: backend/LoggingService/Models/Digest.cs
using System;

namespace LoggingService.Models;

public class Digest
{
    public string DigestId { get; set; }
    public DateTime GeneratedDate { get; set; }
    public int ErrorCount { get; set; }
    public int OutstandingCount { get; set; }
    public bool SentStatus { get; set; }
    public string Recipients { get; set; }  // JSON array of email addresses
}
