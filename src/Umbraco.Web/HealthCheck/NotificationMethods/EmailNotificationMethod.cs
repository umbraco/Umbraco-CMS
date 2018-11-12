using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    [HealthCheckNotificationMethod("email")]
    public class EmailNotificationMethod : NotificationMethodBase, IHealthCheckNotificatationMethod
    {
        private readonly ILocalizedTextService _textService;

        /// <summary>
        /// Default constructor which is used in the provider model
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="failureOnly"></param>
        /// <param name="verbosity"></param>
        /// <param name="recipientEmail"></param>
        public EmailNotificationMethod(bool enabled, bool failureOnly, HealthCheckNotificationVerbosity verbosity,
                string recipientEmail)
            : this(enabled, failureOnly, verbosity, recipientEmail, ApplicationContext.Current.Services.TextService)
        {
        }

        /// <summary>
        /// Constructor that could be used for testing
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="failureOnly"></param>
        /// <param name="verbosity"></param>
        /// <param name="recipientEmail"></param>
        /// <param name="textService"></param>
        internal EmailNotificationMethod(bool enabled, bool failureOnly, HealthCheckNotificationVerbosity verbosity,
            string recipientEmail, ILocalizedTextService textService)
            : base(enabled, failureOnly, verbosity)
        {
            if (textService == null) throw new ArgumentNullException("textService");
            _textService = textService;
            RecipientEmail = recipientEmail;
            Verbosity = verbosity;
        }

        public string RecipientEmail { get; private set; }

        public async Task SendAsync(HealthCheckResults results)
        {
            if (ShouldSend(results) == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(RecipientEmail))
            {
                return;
            }

            var message = _textService.Localize("healthcheck/scheduledHealthCheckEmailBody", new[]
            {
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToShortTimeString(),
                results.ResultsAsHtml(Verbosity)
            });

            // Include the umbraco Application URL host in the message subject so that
            // you can identify the site that these results are for.

            string umbracoApplicationUrl = UmbracoConfig.For.UmbracoSettings().WebRouting.UmbracoApplicationUrl;
            string host = string.Empty;

            if (string.IsNullOrEmpty(umbracoApplicationUrl) == false)
            {
                try
                {
                    Uri umbracoApplicationUri = new Uri(umbracoApplicationUrl);
                    host = umbracoApplicationUri.Host;
                }
                catch (UriFormatException uriFormatException)
                {
                    LogHelper.WarnWithException<EmailNotificationMethod>(string.Format("{0} is not a valid URL for umbracoApplicationUrl", umbracoApplicationUrl), uriFormatException);
                }
            }

            string subject = string.Format(_textService.Localize("healthcheck/scheduledHealthCheckEmailSubject"), host);

            var mailSender = new EmailSender();
            using (var mailMessage = new MailMessage(UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress,
                RecipientEmail,
                string.IsNullOrEmpty(subject) ? "Umbraco Health Check Status" : subject,
                message)
            {
                IsBodyHtml = message.IsNullOrWhiteSpace() == false
                             && message.Contains("<") && message.Contains("</")
            })
            {
                await mailSender.SendAsync(mailMessage);
            }
        }
    }
}
