// File: backend/NotificationService/Services/EmailSender.cs
using System.Threading.Tasks;

namespace NotificationService.Services;

public class EmailSender
{
    // TODO: Add dependencies via constructor injection
    
    public async Task SendDigestEmailAsync(DigestModel digest, string[] recipients)
    {
        // TODO: Implement email sending logic via SMTP (smtp.outlook.com)
    }
}