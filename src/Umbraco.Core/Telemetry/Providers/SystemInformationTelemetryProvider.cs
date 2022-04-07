using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    internal class SystemInformationTelemetryProvider : IDetailedTelemetryProvider, ISystemInformationTableDataProvider
    {
        private readonly IUmbracoVersion _version;
        private readonly ILocalizationService _localizationService;
        private readonly ModelsBuilderSettings _modelsBuilderSettings;

        public SystemInformationTelemetryProvider(
            IUmbracoVersion version,
            ILocalizationService localizationService,
            IOptions<ModelsBuilderSettings> modelsBuilderSettings)
        {
            _version = version;
            _localizationService = localizationService;
            _modelsBuilderSettings = modelsBuilderSettings.Value;
        }

        private string CurrentWebServer => IsRunningInProcessIIS() ? "IIS" : "Kestrel";

        private string ServerFramework => RuntimeInformation.FrameworkDescription;

        private string ModelsBuilderMode => _modelsBuilderSettings.ModelsMode.ToString();

        public IEnumerable<UsageInformation> GetInformation() => throw new System.NotImplementedException();

        public IEnumerable<UserData> GetSystemInformationTableData() =>
            new List<UserData>
            {
                new("Server OS", RuntimeInformation.OSDescription),
                new("Server Framework", ServerFramework),
                new("Default Language", _localizationService.GetDefaultLanguageIsoCode()),
                new("Umbraco Version", _version.SemanticVersion.ToSemanticStringWithoutBuild()),
                new("Current Culture", Thread.CurrentThread.CurrentCulture.ToString()),
                new("Current UI Culture", Thread.CurrentThread.CurrentUICulture.ToString()),
                new("Current Webserver", CurrentWebServer),
                new("Models Builder Mode", ModelsBuilderMode),
            };

        private bool IsRunningInProcessIIS()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            string processName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
            return (processName.Contains("w3wp") || processName.Contains("iisexpress"));
        }
    }
}
