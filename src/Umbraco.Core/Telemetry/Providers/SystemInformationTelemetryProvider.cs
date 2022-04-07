using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    internal class SystemInformationTelemetryProvider : IDetailedTelemetryProvider, IUserDataService
    {
        private readonly IUmbracoVersion _version;
        private readonly ILocalizationService _localizationService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly GlobalSettings _globalSettings;
        private readonly HostingSettings _hostingSettings;
        private readonly ModelsBuilderSettings _modelsBuilderSettings;

        public SystemInformationTelemetryProvider(
            IUmbracoVersion version,
            ILocalizationService localizationService,
            IOptions<ModelsBuilderSettings> modelsBuilderSettings,
            IOptions<HostingSettings> hostingSettings,
            IOptions<GlobalSettings> globalSettings,
            IHostEnvironment hostEnvironment)
        {
            _version = version;
            _localizationService = localizationService;
            _hostEnvironment = hostEnvironment;
            _globalSettings = globalSettings.Value;
            _hostingSettings = hostingSettings.Value;
            _modelsBuilderSettings = modelsBuilderSettings.Value;
        }

        private string CurrentWebServer => IsRunningInProcessIIS() ? "IIS" : "Kestrel";

        private string ServerFramework => RuntimeInformation.FrameworkDescription;

        private string ModelsBuilderMode => _modelsBuilderSettings.ModelsMode.ToString();

        private string CurrentCulture => Thread.CurrentThread.CurrentCulture.ToString();

        private bool IsDebug => _hostingSettings.Debug;

        private bool UmbracoPathCustomized => _globalSettings.UmbracoPath != GlobalSettings.StaticUmbracoPath;

        private string AspEnvironment => _hostEnvironment.EnvironmentName;

        private string ServerOs => RuntimeInformation.OSDescription;

        public IEnumerable<UsageInformation> GetInformation() =>
            new UsageInformation[]
            {
                new("ServerOs", ServerOs),
                new("ServerFramework", ServerFramework),
                new("OsLanguage", CurrentCulture),
                new("Webserver", CurrentWebServer),
                new("ModelsBuilderMode", ModelsBuilderMode),
                new("CustomUmbracoPath", UmbracoPathCustomized),
                new("AspEnvironment", AspEnvironment),
                new("IsDebug", IsDebug),
            };

        public IEnumerable<UserData> GetUserData() =>
            new UserData[]
            {
                new("Server OS", ServerOs),
                new("Server Framework", ServerFramework),
                new("Default Language", _localizationService.GetDefaultLanguageIsoCode()),
                new("Umbraco Version", _version.SemanticVersion.ToSemanticStringWithoutBuild()),
                new("Current Culture", CurrentCulture),
                new("Current UI Culture", Thread.CurrentThread.CurrentUICulture.ToString()),
                new("Current Webserver", CurrentWebServer),
                new("Models Builder Mode", ModelsBuilderMode),
                new("Debug Mode", IsDebug.ToString()),
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
