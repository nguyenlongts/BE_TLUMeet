using System.Net;
using System.Net.Mail;
using NotificationService.API.Model;

namespace NotificationService.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly NotificationDbContext _dbContext;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, NotificationDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task SendEmailAsync(string to, string subject, string body, string type)
    {
        var log = new EmailLog
        {
            RecipientEmail = to,
            Subject = subject,
            Type = type,
            SentAt = DateTime.UtcNow
        };

        try
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUsername = _configuration["Smtp:Username"];
            var smtpPassword = _configuration["Smtp:Password"];

            using var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("noreply@tlumeet.com", "TLUMeet Support"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);

            log.IsSuccess = true;
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            log.IsSuccess = false;
            log.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
        finally
        {
            await _dbContext.EmailLogs.AddAsync(log);
            await _dbContext.SaveChangesAsync();
        }
    }
}
