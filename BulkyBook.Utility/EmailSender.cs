using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public string? Email { get; set; }
        public string? AppPassword { get; set; }

        public EmailSender(IConfiguration _config)
        {
            Email = _config.GetValue<string>("EmailSender:Email");
            AppPassword = _config.GetValue<string>("EmailSender:AppPassword");
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var mailMessage = new MailMessage(from: Email ?? "mohamedashrafmahmoudgad@gmail.com", to: email, subject, message)
            {
                IsBodyHtml = true
            };

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Email ?? "mohamedashrafmahmoudgad@gmail.com", AppPassword ?? "0000")
            };

            return client.SendMailAsync(mailMessage);
        }
    }
}
