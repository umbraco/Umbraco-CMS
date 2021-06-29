using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("3E2F7B14-4B41-452B-9A30-E67FBC8E1206", "Notification Email Settings",
        Description = "If notifications are used, the 'from' email address should be specified and changed from the default value.",
        Group = "Configuration")]
    public class NotificationEmailCheck : AbstractConfigCheck
    {
        private const string DefaultFromEmail = "your@email.here";

        public NotificationEmailCheck(ILocalizedTextService textService)
            : base(textService)
        { }

        public override string FilePath => "~/Config/umbracoSettings.config";

        public override string XPath => "/settings/content/notifications/email";

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldNotEqual;

        public override ProvidedValueValidation ProvidedValueValidation => ProvidedValueValidation.Email;

        public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
        {
            new AcceptableConfiguration { IsRecommended = false, Value = DefaultFromEmail }
        };

        public override string CheckSuccessMessage => TextService.Localize("healthcheck", "notificationEmailsCheckSuccessMessage", new [] { CurrentValue } );

        public override string CheckErrorMessage => TextService.Localize("healthcheck", "notificationEmailsCheckErrorMessage", new[] { DefaultFromEmail });
    }
}
