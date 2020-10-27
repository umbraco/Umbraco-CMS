using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Infrastructure.HealthCheck;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    [HealthCheckNotificationMethod("email")]
    public class EmailNotificationMethod : NotificationMethodBase
    {
        private readonly ILocalizedTextService _textService;
        private readonly IRequestAccessor _requestAccessor;

        private readonly GlobalSettings _globalSettings;
        private readonly ContentSettings _contentSettings;

        public EmailNotificationMethod(
            ILocalizedTextService textService,
            IRequestAccessor requestAccessor,
            IOptions<GlobalSettings> globalSettings,
            IOptions<HealthChecksSettings> healthChecksSettings,
            IOptions<ContentSettings> contentSettings)
            : base(healthChecksSettings)
        {
            var recipientEmail = Settings?["recipientEmail"]?.Value;
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                Enabled = false;
                return;
            }

            RecipientEmail = recipientEmail;

            _textService = textService ?? throw new ArgumentNullException(nameof(textService));
            _requestAccessor = requestAccessor;
            _globalSettings = globalSettings.Value;
            _contentSettings = contentSettings.Value ?? throw new ArgumentNullException(nameof(contentSettings));
        }

        public string RecipientEmail { get; }

        public override async Task SendAsync(HealthCheckResults results)
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
            var host = _requestAccessor.GetApplicationUrl();

            var subject = _textService.Localize("healthcheck/scheduledHealthCheckEmailSubject", new[] { host.ToString() });

            var mailSender = new EmailSender(Options.Create(_globalSettings));
            var mailMessage = CreateMailMessage(subject, message);
            await mailSender.SendAsync(mailMessage);
        }

        private EmailMessage CreateMailMessage(string subject, string message)
        {
            var to = _contentSettings.Notifications.Email;

            if (string.IsNullOrWhiteSpace(subject))
                subject = "Umbraco Health Check Status";

            var isBodyHtml = message.IsNullOrWhiteSpace() == false && message.Contains("<") && message.Contains("</");
            return new EmailMessage(to, RecipientEmail, subject, message, isBodyHtml);
        }
    }
}
