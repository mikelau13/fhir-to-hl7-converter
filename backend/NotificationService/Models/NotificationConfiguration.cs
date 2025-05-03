// File: backend/NotificationService/Models/NotificationConfiguration.cs
namespace NotificationService.Models;

public class EmailOptions
{
    public string SmtpServer { get; set; } = "smtp.outlook.com";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; }
    public string Password { get; set; }
    public string SenderEmail { get; set; }
    public string SenderName { get; set; } = "Ontario PCR Integration";
    public bool EnableSsl { get; set; } = true;
}

public class DigestOptions
{
    public string[] Recipients { get; set; }
    public string LoggingServiceUrl { get; set; }
    public string TransmissionServiceUrl { get; set; }
    public int DigestHour { get; set; } = 7; // 7 AM daily
}
