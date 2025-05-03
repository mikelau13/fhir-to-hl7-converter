// File: backend/NotificationService/Services/EmailSender.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using NotificationService.Models;

namespace NotificationService.Services;

public interface IEmailSender
{
    Task SendDigestEmailAsync(DigestModel digest, string[] recipients);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptions<EmailOptions> options,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendDigestEmailAsync(DigestModel digest, string[] recipients)
    {
        try
        {
            using var client = new SmtpClient(_options.SmtpServer, _options.SmtpPort)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_options.SenderEmail, _options.SenderName),
                Subject = $"Ontario PCR Integration - Daily Digest {digest.GeneratedDate:yyyy-MM-dd}",
                Body = BuildDigestEmailBody(digest),
                IsBodyHtml = true
            };

            foreach (var recipient in recipients)
            {
                mailMessage.To.Add(recipient);
            }

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Sent digest email {DigestId} to {RecipientCount} recipients", 
                digest.DigestId, recipients.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send digest email {DigestId}", digest.DigestId);
            throw;
        }
    }

    private string BuildDigestEmailBody(DigestModel digest)
    {
        var body = new System.Text.StringBuilder();
        
        body.AppendLine("<!DOCTYPE html>");
        body.AppendLine("<html>");
        body.AppendLine("<head>");
        body.AppendLine("<style>");
        body.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; }");
        body.AppendLine("table { border-collapse: collapse; width: 100%; }");
        body.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; }");
        body.AppendLine("th { background-color: #f2f2f2; }");
        body.AppendLine(".error { color: #e74c3c; }");
        body.AppendLine(".warning { color: #f39c12; }");
        body.AppendLine(".success { color: #27ae60; }");
        body.AppendLine(".header { background-color: #3498db; color: white; padding: 10px; }");
        body.AppendLine("</style>");
        body.AppendLine("</head>");
        body.AppendLine("<body>");
        
        body.AppendLine($"<div class='header'><h2>Ontario PCR Integration - Daily Digest</h2></div>");
        body.AppendLine($"<p>Date: {digest.GeneratedDate:yyyy-MM-dd}</p>");
        
        // Summary section
        body.AppendLine("<h3>Summary</h3>");
        body.AppendLine("<ul>");
        body.AppendLine($"<li><strong>Error Count:</strong> {digest.ErrorCount}</li>");
        body.AppendLine($"<li><strong>Outstanding Messages:</strong> {digest.OutstandingCount}</li>");
        body.AppendLine("</ul>");
        
        // Connectivity errors section
        if (digest.ConnectivityErrors?.Count > 0)
        {
            body.AppendLine("<h3>Connectivity Errors</h3>");
            body.AppendLine("<table>");
            body.AppendLine("<tr><th>Timestamp</th><th>Error</th></tr>");
            
            foreach (var error in digest.ConnectivityErrors)
            {
                body.AppendLine("<tr>");
                body.AppendLine($"<td>{error.Timestamp:yyyy-MM-dd HH:mm:ss}</td>");
                body.AppendLine($"<td class='error'>{error.Message}</td>");
                body.AppendLine("</tr>");
            }
            
            body.AppendLine("</table>");
        }
        
        // NACK responses section
        if (digest.NackResponses?.Count > 0)
        {
            body.AppendLine("<h3>NACK Responses</h3>");
            body.AppendLine("<table>");
            body.AppendLine("<tr><th>Message ID</th><th>Patient ID</th><th>Timestamp</th><th>Error</th></tr>");
            
            foreach (var nack in digest.NackResponses)
            {
                body.AppendLine("<tr>");
                body.AppendLine($"<td>{nack.MessageId}</td>");
                body.AppendLine($"<td>{nack.PatientId}</td>");
                body.AppendLine($"<td>{nack.Timestamp:yyyy-MM-dd HH:mm:ss}</td>");
                body.AppendLine($"<td class='error'>{nack.ErrorDetails}</td>");
                body.AppendLine("</tr>");
            }
            
            body.AppendLine("</table>");
        }
        
        // Outstanding messages section
        if (digest.OutstandingMessages?.Count > 0)
        {
            body.AppendLine("<h3>Outstanding Messages</h3>");
            body.AppendLine("<table>");
            body.AppendLine("<tr><th>Message ID</th><th>Patient ID</th><th>Clinic</th><th>Created</th><th>Status</th></tr>");
            
            foreach (var message in digest.OutstandingMessages)
            {
                body.AppendLine("<tr>");
                body.AppendLine($"<td>{message.MessageId}</td>");
                body.AppendLine($"<td>{message.PatientId}</td>");
                body.AppendLine($"<td>{message.ClinicName}</td>");
                body.AppendLine($"<td>{message.CreatedAt:yyyy-MM-dd HH:mm:ss}</td>");
                body.AppendLine($"<td class='warning'>{message.Status}</td>");
                body.AppendLine("</tr>");
            }
            
            body.AppendLine("</table>");
        }
        
        body.AppendLine("<p>This is an automated message. Please do not reply to this email.</p>");
        body.AppendLine("</body>");
        body.AppendLine("</html>");
        
        return body.ToString();
    }
}
