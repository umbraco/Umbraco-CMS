using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("D0F7599E-9B2A-4D9E-9883-81C7EDC5616F", "Macro errors",
        Description = "Checks to make sure macro errors are not set to throw a YSOD (yellow screen of death), which would prevent certain or all pages from loading completely.",
        Group = "Configuration")]
    public class MacroErrorsCheck : AbstractConfigCheck
    {
        public MacroErrorsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext) { }

        public override string FilePath
        {
            get { return "~/Config/umbracoSettings.config"; }
        }

        public override string XPath
        {
            get { return "/settings/content/MacroErrors"; }
        }

        public override ValueComparisonType ValueComparisonType
        {
            get { return ValueComparisonType.ShouldEqual; }
        }

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
            get { return string.Format("MacroErrors are set to '{0}'.", CurrentValue); }
        }

        public override string CheckErrorMessage
        {
            get
            {
                var rectifiedValue = Values.First(x => x.IsRecommended).Value;
                return string.Format("MacroErrors are set to '{0}' which will prevent some or all pages in your site from loading completely when there's any errors in macros. Rectifying this will set the value to '{1}'", CurrentValue, rectifiedValue);
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                var rectifiedValue = Values.First(x => x.IsRecommended).Value;
                return string.Format("MacroErrors are now set to '{0}'", rectifiedValue);
            }
        }
    }
}