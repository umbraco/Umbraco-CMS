using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [HealthCheck("4090C0A1-2C52-4124-92DD-F028FD066A64", "Custom Errors",
        Description = "Leaving custom errors off will display a complete stack trace to your visitors if an exception occurs.",
        Group = "Live Environment")]
    public class CustomErrorsCheck : AbstractConfigCheck
    {
        public CustomErrorsCheck(IConfiguration configuration, ILocalizedTextService textService, ILogger logger)
            : base(configuration, textService, logger)
        { }

        public override string Key => "/configuration/system.web/customErrors/@mode";

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
