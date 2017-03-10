using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("4090C0A1-2C52-4124-92DD-F028FD066A64", "Custom Errors",
        Description = "Leaving custom errors off will display a complete stack trace to your visitors if an exception occurs.",
        Group = "Live Environment")]
    public class CustomErrorsCheck : AbstractConfigCheck
    {
        private readonly ILocalizedTextService _textService;

        public CustomErrorsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override string FilePath
        {
            get { return "~/Web.config"; }
        }

        public override string XPath
        {
            get { return "/configuration/system.web/customErrors/@mode"; }
        }

        public override ValueComparisonType ValueComparisonType
        {
            get { return ValueComparisonType.ShouldEqual; }
        }

        public override IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                return new List<AcceptableConfiguration>
                {
                    new AcceptableConfiguration { IsRecommended = true, Value = CustomErrorsMode.RemoteOnly.ToString() },
                    new AcceptableConfiguration { IsRecommended = false, Value = "On" }
                };
            }
        }

        public override string CheckSuccessMessage
        {
            get
            {
                return _textService.Localize("healthcheck/customErrorsCheckSuccessMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value });
            }
        }

        public override string CheckErrorMessage
        {
            get
            {
                return _textService.Localize("healthcheck/customErrorsCheckErrorMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value });
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                return _textService.Localize("healthcheck/customErrorsCheckRectifySuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value });
            }
        }
    }
}