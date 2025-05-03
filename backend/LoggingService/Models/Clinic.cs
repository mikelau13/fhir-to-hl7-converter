// File: backend/LoggingService/Models/Clinic.cs
namespace LoggingService.Models;

public class Clinic
{
    public string ClinicId { get; set; }
    public string ClinicName { get; set; }
    public bool IsActive { get; set; }
    public string Settings { get; set; }
}
