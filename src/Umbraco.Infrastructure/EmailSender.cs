// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Extensions;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Umbraco.Cms.Infrastructure
{
    /// <summary>
    /// A utility class for sending emails
    /// </summary>
    public class EmailSender : IEmailSender
    {
        // TODO: This should encapsulate a BackgroundTaskRunner with a queue to send these emails!

        private readonly GlobalSettings _globalSettings;

        public EmailSender(IOptions<GlobalSettings> globalSettings) => _globalSettings = globalSettings.Value;

        /// <summary>
        /// Sends the message async
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendAsync(EmailMessage message)
        {
            if (_globalSettings.IsSmtpServerConfigured == false)
            {
                return;
            }

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_globalSettings.Smtp.Host,
                    _globalSettings.Smtp.Port,
                    (MailKit.Security.SecureSocketOptions)(int)_globalSettings.Smtp.SecureSocketOptions);

                if (!(_globalSettings.Smtp.Username is null && _globalSettings.Smtp.Password is null))
                {
                    await client.AuthenticateAsync(_globalSettings.Smtp.Username, _globalSettings.Smtp.Password);
                }

                var mailMessage = message.ToMimeMessage(_globalSettings.Smtp.From);
                if (_globalSettings.Smtp.DeliveryMethod == SmtpDeliveryMethod.Network)
                {
                    await client.SendAsync(mailMessage);
                }
                else
                {
                    client.Send(mailMessage);
                }

                await client.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Returns true if the application should be able to send a required application email
        /// </summary>
        /// <remarks>
        /// We assume this is possible if either an event handler is registered or an smtp server is configured
        /// </remarks>
        public bool CanSendRequiredEmail() => _globalSettings.IsSmtpServerConfigured;
    }
}
