using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Core.HealthCheck.Checks.LiveEnvironment
{
    [HealthCheck("9BED6EF4-A7F3-457A-8935-B64E9AA8BAB3", "Trace Mode",
        Description = "Leaving trace mode enabled can make valuable information about your system available to hackers.",
        Group = "Live Environment")]
    public class TraceCheck : AbstractSettingsCheck
    {
        public TraceCheck(ILocalizedTextService textService, ILoggerFactory loggerFactory, IConfigurationService configurationService)
        : base(textService, loggerFactory, configurationService)
        { }

        public override string ItemPath => "/configuration/system.web/trace/@enabled";

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

        public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
        {
            new AcceptableConfiguration { IsRecommended = true, Value = bool.FalseString.ToLower() }
        };

        public override string CurrentValue { get; }

        public override string CheckSuccessMessage => TextService.Localize("healthcheck/traceModeCheckSuccessMessage");

        public override string CheckErrorMessage => TextService.Localize("healthcheck/traceModeCheckErrorMessage");

        public override string RectifySuccessMessage => TextService.Localize("healthcheck/traceModeCheckRectifySuccessMessage");
        
    }
}
