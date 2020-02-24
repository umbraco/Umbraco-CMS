using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;

namespace Umbraco.Core
{
    /// <summary>
    /// A utility class for sending emails
    /// </summary>
    public class EmailSender : IEmailSender
    {
        // TODO: This should encapsulate a BackgroundTaskRunner with a queue to send these emails!

        private readonly IGlobalSettings _globalSettings;
        private readonly bool _enableEvents;

        public EmailSender(IGlobalSettings globalSettings) : this(globalSettings, false)
        {
        }

        internal EmailSender(IGlobalSettings globalSettings, bool enableEvents)
        {
            _globalSettings = globalSettings;
            _enableEvents = enableEvents;

            _smtpConfigured = new Lazy<bool>(() => _globalSettings.IsSmtpServerConfigured);
        }

        private readonly Lazy<bool> _smtpConfigured;

        /// <summary>
        /// Sends the message non-async
        /// </summary>
        /// <param name="message"></param>
        public void Send(MailMessage message)
        {
            if (_smtpConfigured.Value == false && _enableEvents)
            {
                OnSendEmail(new SendEmailEventArgs(message));
            }
            else
            {
                using (var client = new SmtpClient())
                {
                    client.Send(message);
                }
            }
        }

        /// <summary>
        /// Sends the message async
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendAsync(MailMessage message)
        {
            if (_smtpConfigured.Value == false && _enableEvents)
            {
                OnSendEmail(new SendEmailEventArgs(message));
            }
            else
            {
                using (var client = new SmtpClient())
                {
                    if (client.DeliveryMethod == SmtpDeliveryMethod.Network)
                    {
                        await client.SendMailAsync(message);
                    }
                    else
                    {
                        client.Send(message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the application should be able to send a required application email
        /// </summary>
        /// <remarks>
        /// We assume this is possible if either an event handler is registered or an smtp server is configured
        /// </remarks>
        internal static bool CanSendRequiredEmail(IGlobalSettings globalSettings) => EventHandlerRegistered || globalSettings.IsSmtpServerConfigured;

        /// <summary>
        /// returns true if an event handler has been registered
        /// </summary>
        internal static bool EventHandlerRegistered
        {
            get { return SendEmail != null; }
        }

        /// <summary>
        /// An event that is raised when no smtp server is configured if events are enabled
        /// </summary>
        internal static event EventHandler<SendEmailEventArgs> SendEmail;

        private static void OnSendEmail(SendEmailEventArgs e)
        {
            var handler = SendEmail;
            if (handler != null) handler(null, e);
        }
    }
}
