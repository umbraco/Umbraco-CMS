using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Security
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            using (var client = new SmtpClient())
            using (var mailMessage = new MailMessage(
                UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress, 
                message.Destination,
                message.Subject,
                message.Body))
            {                
                mailMessage.IsBodyHtml = message.Body.IsNullOrWhiteSpace() == false 
                    && message.Body.Contains("<") && message.Body.Contains("</");

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
