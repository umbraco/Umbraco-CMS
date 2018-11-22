using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("3E2F7B14-4B41-452B-9A30-E67FBC8E1206", "Notification Email Settings",
        Description = "If notifications are used, the 'from' email address should be specified and changed from the default value.",
        Group = "Configuration")]
    public class NotificationEmailCheck : AbstractConfigCheck
    {
        private readonly ILocalizedTextService _textService;
        private const string DefaultFromEmail = "your@email.here";

        public NotificationEmailCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override string FilePath
        {
            get { return "~/Config/umbracoSettings.config"; }
        }

        public override string XPath
        {
            get { return "/settings/content/notifications/email"; }
        }

        public override ValueComparisonType ValueComparisonType
        {
            get { return ValueComparisonType.ShouldNotEqual; }
        }

        public override IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                return new List<AcceptableConfiguration>
                {
                    new AcceptableConfiguration { IsRecommended = false, Value = DefaultFromEmail }
                };
            }
        }

        public override string CheckSuccessMessage
        {
            get { return _textService.Localize("healthcheck/notificationEmailsCheckSuccessMessage", new [] { CurrentValue } ); }
        }

        public override string CheckErrorMessage
        {
            get { return _textService.Localize("healthcheck/notificationEmailsCheckErrorMessage", new[] { DefaultFromEmail }); }
        }
    }
}