using Microsoft.Extensions.Configuration;
using YourAppName.Service.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YourAppName.Data.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace YourAppName.Service.Implementations
{
    public class EmailService : IEmailService
    {
        #region Fields
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        #endregion

        #region Constructor
        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }
        #endregion

        #region Methods
        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.Email, "YourAppName App"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                // Try to send
                await client.SendMailAsync(mailMessage, cancellationToken);
            }
            catch (SmtpException ex)
            {
                // Catch specific Email server errors (e.g., wrong password, blocked port)
                _logger.LogError(ex, "SMTP error occurred while sending email to {Email}", toEmail);

                // Note: We are NOT throwing the exception here. 
                // The app will continue running gracefully.
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                _logger.LogError(ex, "An unexpected error occurred while sending email to {Email}", toEmail);
            }
        }
        #endregion
    }
}
