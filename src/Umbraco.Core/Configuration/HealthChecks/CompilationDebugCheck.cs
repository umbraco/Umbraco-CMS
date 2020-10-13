using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.HealthCheck.Checks.LiveEnvironment
{
    [HealthCheck("61214FF3-FC57-4B31-B5CF-1D095C977D6D", "Debug Compilation Mode",
        Description = "Leaving debug compilation mode enabled can severely slow down a website and take up more memory on the server.",
        Group = "Live Environment")]
    public class CompilationDebugCheck : AbstractSettingsCheck
    {
        private readonly HostingSettings _hostingSettings;

        public CompilationDebugCheck(ILocalizedTextService textService, ILoggerFactory loggerFactory,IConfigurationService configurationService, IOptions<HostingSettings> hostingSettings)
            : base(textService, loggerFactory, configurationService)
        {
            _hostingSettings = hostingSettings.Value;
        }

        public override string ItemPath => Constants.Configuration.ConfigHostingDebug;

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

        public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
        {
            new AcceptableConfiguration
            {
                IsRecommended = true,
                Value = bool.FalseString.ToLower()
            }
        };

        public override string CurrentValue => _hostingSettings.Debug.ToString();

        public override string CheckSuccessMessage => TextService.Localize("healthcheck/compilationDebugCheckSuccessMessage");

        public override string CheckErrorMessage => TextService.Localize("healthcheck/compilationDebugCheckErrorMessage");

        public override string RectifySuccessMessage => TextService.Localize("healthcheck/compilationDebugCheckRectifySuccessMessage");
    }
}
