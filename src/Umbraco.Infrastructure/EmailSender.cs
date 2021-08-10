// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Notifications;
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
        private readonly IEventAggregator _eventAggregator;
        private readonly GlobalSettings _globalSettings;
        private readonly bool _notificationHandlerRegistered;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(
            ILogger<EmailSender> logger,
            IOptions<GlobalSettings> globalSettings,
            IEventAggregator eventAggregator)
            : this(globalSettings, eventAggregator, null)
            => _logger = logger;

        public EmailSender(IOptions<GlobalSettings> globalSettings, IEventAggregator eventAggregator, INotificationHandler<SendEmailNotification> handler)
        {
            _eventAggregator = eventAggregator;
            _globalSettings = globalSettings.Value;
            _notificationHandlerRegistered = handler is not null;
        }

        /// <summary>
        /// Sends the message async
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendAsync(EmailMessage message) => await SendAsyncInternal(message, false);

        public async Task SendAsync(EmailMessage message, bool enableNotification) =>
            await SendAsyncInternal(message, enableNotification);

        private async Task SendAsyncInternal(EmailMessage message, bool enableNotification)
        {
            if (enableNotification)
            {
                var notification = new SendEmailNotification(message.ToNotificationEmail(_globalSettings.Smtp?.From));
                await _eventAggregator.PublishAsync(notification);

                // if a handler handled sending the email then don't continue.
                if (notification.IsHandled)
                {
                    _logger.LogDebug("The email sending for {Subject} was handled by a notification handler", notification.Message.Subject);
                    return;
                }
            }

            if (_globalSettings.IsSmtpServerConfigured == false)
            {
                _logger.LogDebug("Could not send email for {Subject}. It was not handled by a notification handler and there is no SMTP configured.", message.Subject);
                return;
            }

            using var client = new SmtpClient();

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

        /// <summary>
        /// Returns true if the application should be able to send a required application email
        /// </summary>
        /// <remarks>
        /// We assume this is possible if either an event handler is registered or an smtp server is configured
        /// </remarks>
        public bool CanSendRequiredEmail() => _globalSettings.IsSmtpServerConfigured || _notificationHandlerRegistered;
    }
}
