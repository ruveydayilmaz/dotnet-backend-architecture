using Business.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.Results;

namespace Business.Concrete
{
    public class EmailManager : IEmailService
    {
        public async Task SendEmail(EmailRequest emailRequest)
        {
            try
            {
                var smtpClient = new SmtpClient("your-smtp-server.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("your-email@example.com", "your-email-password"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("your-email@example.com"),
                    Subject = "Subject of the email",
                    Body = $"Your verification code is: {emailRequest.Code}",
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(emailRequest.ToUser);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                new ErrorResult($"Error sending email: {ex}");
            }
        }
    }
}
