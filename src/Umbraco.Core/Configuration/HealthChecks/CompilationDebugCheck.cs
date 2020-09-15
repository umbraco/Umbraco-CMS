using System.Collections.Generic;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [HealthCheck("61214FF3-FC57-4B31-B5CF-1D095C977D6D", "Debug Compilation Mode",
        Description = "Leaving debug compilation mode enabled can severely slow down a website and take up more memory on the server.",
        Group = "Live Environment")]
    public class CompilationDebugCheck : AbstractSettingsCheck
    {
        public CompilationDebugCheck(ILocalizedTextService textService, ILogger logger,IConfigurationService configurationService)
            : base(textService, logger, configurationService)
        { }
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            throw new System.NotImplementedException();
        }

        public override string ItemPath => Constants.Configuration.ConfigHostingDebug;

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

        public override bool ValidIfConfigMissing => true;

        public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
        {
            new AcceptableConfiguration
            {
                IsRecommended = true,
                Value = bool.FalseString.ToLower()
            }
        };

        public override string CheckSuccessMessage => TextService.Localize("healthcheck/compilationDebugCheckSuccessMessage");

        public override string CheckErrorMessage => TextService.Localize("healthcheck/compilationDebugCheckErrorMessage");

        public override string RectifySuccessMessage => TextService.Localize("healthcheck/compilationDebugCheckRectifySuccessMessage");
    }
}
