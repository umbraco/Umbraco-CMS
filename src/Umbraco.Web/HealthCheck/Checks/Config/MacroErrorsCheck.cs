using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("D0F7599E-9B2A-4D9E-9883-81C7EDC5616F", "Macro errors",
        Description = "Checks to make sure macro errors are not set to throw a YSOD (yellow screen of death), which would prevent certain or all pages from loading completely.",
        Group = "Configuration")]
    public class MacroErrorsCheck : AbstractConfigCheck
    {
        public MacroErrorsCheck(ILocalizedTextService textService)
            : base(textService)
        { }

        public override string FilePath => "~/Config/umbracoSettings.config";

        public override string XPath => "/settings/content/MacroErrors";

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

        public override IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                var values = new List<AcceptableConfiguration>
                {
                    new AcceptableConfiguration
                    {
                        IsRecommended = true,
                        Value = "inline"
                    },
                    new AcceptableConfiguration
                    {
                        IsRecommended = false,
                        Value = "silent"
                    }
                };

                return values;
            }
        }

        public override string CheckSuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck", "macroErrorModeCheckSuccessMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value });
            }
        }

        public override string CheckErrorMessage
        {
            get
            {
                return TextService.Localize("healthcheck", "macroErrorModeCheckErrorMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value });
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck", "macroErrorModeCheckRectifySuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value });
            }
        }
    }
}
