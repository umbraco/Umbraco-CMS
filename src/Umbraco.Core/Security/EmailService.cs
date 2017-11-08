using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// The <see cref="IIdentityMessageService"/> implementation for Umbraco
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        private readonly string _notificationEmailAddress;
        private readonly IEmailSender _defaultEmailSender;
        
        public EmailService(string notificationEmailAddress, IEmailSender defaultEmailSender)
        {
            _notificationEmailAddress = notificationEmailAddress;
            _defaultEmailSender = defaultEmailSender;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the constructor specifying all dependencies")]
        public EmailService()
            : this(UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress, new EmailSender())
        {
        }

        public async Task SendAsync(IdentityMessage message)
        {
            var mailMessage = new MailMessage(
                _notificationEmailAddress,
                message.Destination,
                message.Subject,
                message.Body)
            {
                IsBodyHtml = message.Body.IsNullOrWhiteSpace() == false
                             && message.Body.Contains("<") && message.Body.Contains("</")
            };

            try
            {
                //check if it's a custom message and if so use it's own defined mail sender
                var umbMsg = message as UmbracoEmailMessage;
                if (umbMsg != null)
                {
                    await umbMsg.MailSender.SendAsync(mailMessage);
                }
                else
                {
                    await _defaultEmailSender.SendAsync(mailMessage);
                }
            }
            finally
            {
                mailMessage.Dispose();
            }
        }
    }
}
