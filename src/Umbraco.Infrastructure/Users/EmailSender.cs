using System;
using System.Net.Mail;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Umbraco.Core
{
    /// <summary>
    /// A utility class for sending emails
    /// </summary>
    public class EmailSender : IEmailSender
    {
        // TODO: This should encapsulate a BackgroundTaskRunner with a queue to send these emails!

        private readonly GlobalSettings _globalSettings;
        private readonly bool _enableEvents;

        public EmailSender(IOptions<GlobalSettings> globalSettings) : this(globalSettings, false)
        {
        }

        public EmailSender(IOptions<GlobalSettings> globalSettings, bool enableEvents)
        {
            _globalSettings = globalSettings.Value;
            _enableEvents = enableEvents;

            _smtpConfigured = new Lazy<bool>(() => _globalSettings.IsSmtpServerConfigured);
        }

        private readonly Lazy<bool> _smtpConfigured;

        /// <summary>
        /// Sends the message non-async
        /// </summary>
        /// <param name="message"></param>
        public void Send(EmailMessage message)
        {
            if (_smtpConfigured.Value == false && _enableEvents)
            {
                OnSendEmail(new SendEmailEventArgs(message));
            }
            else if (_smtpConfigured.Value == true)
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(_globalSettings.Smtp.Host,
                        _globalSettings.Smtp.Port,
                        (MailKit.Security.SecureSocketOptions)(int)_globalSettings.Smtp.SecureSocketOptions);

                    if (!(_globalSettings.Smtp.Username is null && _globalSettings.Smtp.Password is null))
                    {
                        client.Authenticate(_globalSettings.Smtp.Username, _globalSettings.Smtp.Password);
                    }

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
        public async Task SendAsync(EmailMessage message)
        {
            if (_smtpConfigured.Value == false && _enableEvents)
            {
                OnSendEmail(new SendEmailEventArgs(message));
            }
            else if (_smtpConfigured.Value == true)
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_globalSettings.Smtp.Host,
                        _globalSettings.Smtp.Port,
                        (MailKit.Security.SecureSocketOptions)(int)_globalSettings.Smtp.SecureSocketOptions);

                    if (!(_globalSettings.Smtp.Username is null && _globalSettings.Smtp.Password is null))
                    {
                        await client.AuthenticateAsync(_globalSettings.Smtp.Username, _globalSettings.Smtp.Password);
                    }

                    var mailMessage = ConstructEmailMessage(message);
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
        }

        /// <summary>
        /// Returns true if the application should be able to send a required application email
        /// </summary>
        /// <remarks>
        /// We assume this is possible if either an event handler is registered or an smtp server is configured
        /// </remarks>
        public static bool CanSendRequiredEmail(GlobalSettings globalSettings) => EventHandlerRegistered || globalSettings.IsSmtpServerConfigured;

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
            SendEmail?.Invoke(null, e);
        }

        private MimeMessage ConstructEmailMessage(EmailMessage mailMessage)
        {
            var fromEmail = mailMessage.From;
            if(string.IsNullOrEmpty(fromEmail))
                fromEmail = _globalSettings.Smtp.From;

            var messageToSend = new MimeMessage
            {
                Subject = mailMessage.Subject,
                From = { MailboxAddress.Parse(fromEmail) },
                Body = new TextPart(mailMessage.IsBodyHtml ? TextFormat.Html : TextFormat.Plain) { Text = mailMessage.Body }
            };
            messageToSend.To.Add(MailboxAddress.Parse(mailMessage.To));

            return messageToSend;
        }
    }
}
