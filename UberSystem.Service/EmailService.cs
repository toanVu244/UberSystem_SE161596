using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;

        public EmailService()
        {
            _smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("toanvtse161596@fpt.edu.vn", "blph emwy whsd gpuc"),
                EnableSsl = true,
            };
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var mailMessage = new MailMessage("toanvtse161596@fpt.edu.vn", email, subject, message);
                await _smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                // Xử lý lỗi liên quan đến SMTP
                throw new Exception($"SMTP error: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }
    }
}
