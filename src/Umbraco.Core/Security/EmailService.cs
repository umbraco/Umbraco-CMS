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
            var mailMessage = new MailMessage(
                UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress,
                message.Destination,
                message.Subject,
                message.Body)
            {
                IsBodyHtml = message.Body.IsNullOrWhiteSpace() == false
                             && message.Body.Contains("<") && message.Body.Contains("</")
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    if (client.DeliveryMethod == SmtpDeliveryMethod.Network)
                    {
                        await client.SendMailAsync(mailMessage);
                    }
                    else
                    {
                        client.Send(mailMessage);
                    }
                }
            }
            finally
            {
                mailMessage.Dispose();
            }
        }
    }
}
