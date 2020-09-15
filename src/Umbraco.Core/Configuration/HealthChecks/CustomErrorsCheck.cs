using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [HealthCheck("4090C0A1-2C52-4124-92DD-F028FD066A64", "Custom Errors",
        Description = "Leaving custom errors off will display a complete stack trace to your visitors if an exception occurs.",
        Group = "Live Environment")]
    public class CustomErrorsCheck : AbstractSettingsCheck
    {
        public CustomErrorsCheck(ILocalizedTextService textService, ILogger logger, IConfigurationService configurationService)
            : base(textService, logger, configurationService)
        { }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            throw new System.NotImplementedException();
        }

        public override string ItemPath => Constants.Configuration.ConfigCustomErrors;

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

        public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
        {
            new AcceptableConfiguration { IsRecommended = true, Value = "RemoteOnly" },
            new AcceptableConfiguration { IsRecommended = false, Value = "On" }
        };

        public override string CheckSuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck/customErrorsCheckSuccessMessage", new[] { CurrentValue });
            }
        }

        public override string CheckErrorMessage
        {
            get
            {
                return TextService.Localize("healthcheck/customErrorsCheckErrorMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value });
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck/customErrorsCheckRectifySuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value });
            }
        }
    }
}
