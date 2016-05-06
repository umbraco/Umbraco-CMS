using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("4090C0A1-2C52-4124-92DD-F028FD066A64", "Custom Errors",
        Description = "Leaving custom errors off will display a complete stack trace to your visitors if an exception occurs.",
        Group = "Live Environment")]
    public class CustomErrorsCheck : AbstractConfigCheck
    {
        public CustomErrorsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext) { }

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
            get { return string.Format("Custom errors are set to '{0}'.", CurrentValue); }
        }

        public override string CheckErrorMessage
        {
            get
            {
                var recommendedValue = Values.First(x => x.IsRecommended).Value;
                return string.Format("Custom errors are currently set to '{0}'. It is recommended to set this to '{1}' before go live.", CurrentValue, recommendedValue);
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                var recommendedValue = Values.First(x => x.IsRecommended).Value;
                return string.Format("Custom errors successfully set to '{0}'.", recommendedValue);
            }
        }
    }
}