using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.HealthCheck.Checks.Configuration
{
    [HealthCheck("3E2F7B14-4B41-452B-9A30-E67FBC8E1206", "Notification Email Settings",
        Description = "If notifications are used, the 'from' email address should be specified and changed from the default value.",
        Group = "Configuration")]
    public class NotificationEmailCheck : AbstractSettingsCheck
    {
        private readonly ContentSettings _contentSettings;
        private const string DefaultFromEmail = "your@email.here";

        public NotificationEmailCheck(ILocalizedTextService textService, ILoggerFactory loggerFactory, IOptions<ContentSettings> contentSettings)
            : base(textService, loggerFactory)
        {
            _contentSettings = contentSettings.Value;
        }

        public override string ItemPath => Constants.Configuration.ConfigContentNotificationsEmail;

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldNotEqual;

        public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
        {
            new AcceptableConfiguration { IsRecommended = false, Value = DefaultFromEmail }
        };

        public override string CurrentValue => _contentSettings.Notifications.Email;

        public override string CheckSuccessMessage => TextService.Localize("healthcheck/notificationEmailsCheckSuccessMessage", new[] { CurrentValue });

        public override string CheckErrorMessage => TextService.Localize("healthcheck/notificationEmailsCheckErrorMessage", new[] { DefaultFromEmail });
       
    }
}
