using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("9BED6EF4-A7F3-457A-8935-B64E9AA8BAB3", "Trace Mode",
        Description = "Leaving trace mode enabled can make valuable information about your system available to hackers.",
        Group = "Live Environment")]
    public class TraceCheck : AbstractConfigCheck
    {

        public TraceCheck(ILocalizedTextService textService)
            : base(textService)
        { }

        public override string FilePath => "~/Web.config";

        public override string XPath => "/configuration/system.web/trace/@enabled";

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

        public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
        {
            new AcceptableConfiguration { IsRecommended = true, Value = bool.FalseString.ToLower() }
        };

        public override string CheckSuccessMessage => TextService.Localize("healthcheck", "traceModeCheckSuccessMessage");

        public override string CheckErrorMessage => TextService.Localize("healthcheck", "traceModeCheckErrorMessage");

        public override string RectifySuccessMessage => TextService.Localize("healthcheck", "traceModeCheckRectifySuccessMessage");
    }
}
