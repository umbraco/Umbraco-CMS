using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [HealthCheck("3E2F7B14-4B41-452B-9A30-E67FBC8E1206", "Notification Email Settings",
        Description = "If notifications are used, the 'from' email address should be specified and changed from the default value.",
        Group = "Configuration")]
    public class NotificationEmailCheck : AbstractConfigCheck
    {
        private const string DefaultFromEmail = "your@email.here";

        public NotificationEmailCheck(IConfiguration configuration, ILocalizedTextService textService, ILogger logger)
            : base(configuration, textService, logger)
        {
        }

        public override string Key => "Umbraco:CMS:Content:Notifications:Email";

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldNotEqual;

        public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
        {
            new AcceptableConfiguration { IsRecommended = false, Value = DefaultFromEmail }
        };

        public override string CheckSuccessMessage => TextService.Localize("healthcheck/notificationEmailsCheckSuccessMessage", new[] { CurrentValue });

        public override string CheckErrorMessage => TextService.Localize("healthcheck/notificationEmailsCheckErrorMessage", new[] { DefaultFromEmail });
    }
}
