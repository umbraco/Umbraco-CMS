using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("046A066C-4FB2-4937-B931-069964E16C66", "Try Skip IIS Custom Errors",
        Description = "Starting with IIS 7.5, this must be set to true for Umbraco 404 pages to show. Else, IIS will takeover and render its built-in error page.",
        Group = "Configuration")]
    public class TrySkipIisCustomErrorsCheck : AbstractConfigCheck
    {
        private readonly Version _serverVersion = HttpRuntime.IISVersion;

        public TrySkipIisCustomErrorsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext) { }

        public override string FilePath
        {
            get { return "~/Config/umbracoSettings.config"; }
        }

        public override string XPath
        {
            get { return "/settings/web.routing/@trySkipIisCustomErrors"; }
        }

        public override ValueComparisonType ValueComparisonType
        {
            get { return ValueComparisonType.ShouldEqual; }
        }

        public override IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                var recommendedValue = _serverVersion >= new Version("7.5.0")
                    ? bool.TrueString.ToLower()
                    : bool.FalseString.ToLower();
                return new List<AcceptableConfiguration> { new AcceptableConfiguration { IsRecommended =  true, Value = recommendedValue } };
            }
        }
        
        public override string CheckSuccessMessage
        {
            get { return string.Format("Try Skip IIS Custom Errors is {0} and you're using IIS version {1}.", CurrentValue, _serverVersion); }
        }

        public override string CheckErrorMessage
        {
            get
            {
                var recommendedValue = Values.First(x => x.IsRecommended).Value;
                return string.Format("Try Skip IIS Custom Errors is currently '{0}'. It is recommended to set this to '{1}' for your IIS version ({2}).", CurrentValue, recommendedValue, _serverVersion);
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                var recommendedValue = Values.First(x => x.IsRecommended).Value;
                return string.Format("Try Skip IIS Custom Errors successfully set to '{0}'.", recommendedValue);
            }
        }
    }
}