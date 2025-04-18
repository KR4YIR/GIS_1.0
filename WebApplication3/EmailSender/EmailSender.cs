using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using WebApplication3.Models;

namespace WebApplication3.EmailSender
{

    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try { 
                using (var smtpClient = new SmtpClient
                {
                    Host = _emailSettings.SmtpServer,
                    Port = _emailSettings.Port,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password)
                })
                {
                    using (var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        mailMessage.To.Add(toEmail);
                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }
            }
            catch(SmtpException ex) { throw new InvalidOperationException("E-posta gönderilemedi", ex); }
        }
    }
}
