using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;

namespace Umbraco.Core
{
    /// <summary>
    /// A utility class for sending emails
    /// </summary>
    public class EmailSender : IEmailSender
    {
        //TODO: This should encapsulate a BackgroundTaskRunner with a queue to send these emails!

        private readonly bool _enableEvents;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EmailSender() : this(false)
        {
        }

        internal EmailSender(bool enableEvents)
        {
            _enableEvents = enableEvents;
        }

        private static readonly Lazy<bool> SmtpConfigured = new Lazy<bool>(() => GlobalSettings.HasSmtpServerConfigured(HttpRuntime.AppDomainAppVirtualPath));

        /// <summary>
        /// Sends the message non-async
        /// </summary>
        /// <param name="message"></param>
        public void Send(MailMessage message)
        {
            if (SmtpConfigured.Value == false && _enableEvents)
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
            if (SmtpConfigured.Value == false && _enableEvents)
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
        internal static bool CanSendRequiredEmail
        {
            get { return EventHandlerRegistered || SmtpConfigured.Value; }
        }

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
