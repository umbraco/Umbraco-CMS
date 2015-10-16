using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Umbraco.Core.Security
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            using (var client = new SmtpClient())
            using (var mailMessage = new MailMessage())
            {
                mailMessage.Body = message.Body;
                mailMessage.To.Add(message.Destination);
                mailMessage.Subject = message.Subject;
                mailMessage.IsBodyHtml = true;

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
