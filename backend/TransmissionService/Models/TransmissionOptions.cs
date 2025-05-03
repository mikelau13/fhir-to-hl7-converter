// File: backend/TransmissionService/Models/TransmissionOptions.cs
namespace TransmissionService.Models;

public class TransmissionOptions
{
    public string EndpointUrl { get; set; }
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 5000;
    public bool UseBackoffStrategy { get; set; } = true;
}
