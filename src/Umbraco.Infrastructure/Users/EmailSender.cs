using System;
using System.Net.Mail;
using System.Threading.Tasks;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

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

        public EmailSender(IGlobalSettings globalSettings, bool enableEvents)
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
                    client.Connect(_globalSettings.SmtpSettings.Host, _globalSettings.SmtpSettings.Port);
                    client.Send(ConstructEmailMessage(message));
                    client.Disconnect(true);
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
                    var smtpSettingsDeliveryMethod = _globalSettings.SmtpSettings.DeliveryMethod;
                    var deliveryMethod = (SmtpDeliveryMethod)Enum.Parse(typeof(SmtpDeliveryMethod), smtpSettingsDeliveryMethod, true);

                    await client.ConnectAsync(_globalSettings.SmtpSettings.Host, _globalSettings.SmtpSettings.Port);

                    if (deliveryMethod == SmtpDeliveryMethod.Network)
                    {
                        await client.SendAsync(ConstructEmailMessage(message));
                    }
                    else
                    {
                        client.Send(ConstructEmailMessage(message));
                    }
                    
                    await client.DisconnectAsync(true);
                }
            }
        }

        /// <summary>
        /// Returns true if the application should be able to send a required application email
        /// </summary>
        /// <remarks>
        /// We assume this is possible if either an event handler is registered or an smtp server is configured
        /// </remarks>
        public static bool CanSendRequiredEmail(IGlobalSettings globalSettings) => EventHandlerRegistered || globalSettings.IsSmtpServerConfigured;

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

        private MimeMessage ConstructEmailMessage(MailMessage mailMessage)
        {
            var messageToSend = new MimeMessage
            {
                Subject = mailMessage.Subject
            };

            var fromEmail = mailMessage.From?.Address;
            if(string.IsNullOrEmpty(fromEmail))
                fromEmail = _globalSettings.SmtpSettings.From;
            
            messageToSend.From.Add(new MailboxAddress(fromEmail));

            foreach (var mailAddress in mailMessage.To)
                messageToSend.To.Add(new MailboxAddress(mailAddress.Address));

            if (mailMessage.IsBodyHtml)
                messageToSend.Body = new TextPart(TextFormat.Html) { Text = mailMessage.Body };
            else
                messageToSend.Body = new TextPart(TextFormat.Plain) { Text = mailMessage.Body };

            return messageToSend;
        }
    }
}
