using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace _240519P_AS_ASSN2.Security
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtp = _config.GetSection("SmtpSettings");

            var client = new SmtpClient(smtp["Host"])
            {
                Port = int.Parse(smtp["Port"]!),
                Credentials = new NetworkCredential(
                    smtp["Username"],
                    smtp["Password"]),
                EnableSsl = bool.Parse(smtp["EnableSsl"]!)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtp["Username"]!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }
    }
}
