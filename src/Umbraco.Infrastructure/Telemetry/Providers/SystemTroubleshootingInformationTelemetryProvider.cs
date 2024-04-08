using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

internal class SystemTroubleshootingInformationTelemetryProvider : IDetailedTelemetryProvider, ISystemTroubleshootingInformationService
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly HostingSettings _hostingSettings;
    private readonly ILocalizationService _localizationService;
    private readonly ModelsBuilderSettings _modelsBuilderSettings;
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private readonly IUmbracoVersion _version;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly RuntimeSettings _runtimeSettings;

    public SystemTroubleshootingInformationTelemetryProvider(
        IUmbracoVersion version,
        ILocalizationService localizationService,
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IOptionsMonitor<HostingSettings> hostingSettings,
        IHostEnvironment hostEnvironment,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IServerRoleAccessor serverRoleAccessor,
        IOptionsMonitor<RuntimeSettings> runtimeSettings)
    {
        _version = version;
        _localizationService = localizationService;
        _hostEnvironment = hostEnvironment;
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _serverRoleAccessor = serverRoleAccessor;
        _runtimeSettings = runtimeSettings.CurrentValue;
        _hostingSettings = hostingSettings.CurrentValue;
        _modelsBuilderSettings = modelsBuilderSettings.CurrentValue;
    }

    private string CurrentWebServer => GetWebServerName();

    private string ServerFramework => RuntimeInformation.FrameworkDescription;

    private string ModelsBuilderMode => _modelsBuilderSettings.ModelsMode.ToString();

    private string RuntimeMode => _runtimeSettings.Mode.ToString();

    private string CurrentCulture => Thread.CurrentThread.CurrentCulture.ToString();

    private bool IsDebug => _hostingSettings.Debug;

    private string AspEnvironment => _hostEnvironment.EnvironmentName;

    private string ServerOs => RuntimeInformation.OSDescription;

    private string DatabaseProvider => _umbracoDatabaseFactory.CreateDatabase().DatabaseType.GetProviderName();

    private string CurrentServerRole => _serverRoleAccessor.CurrentServerRole.ToString();

    public IEnumerable<UsageInformation> GetInformation() =>
        new UsageInformation[]
        {
            new(Constants.Telemetry.ServerOs, ServerOs), new(Constants.Telemetry.ServerFramework, ServerFramework),
            new(Constants.Telemetry.OsLanguage, CurrentCulture),
            new(Constants.Telemetry.WebServer, CurrentWebServer),
            new(Constants.Telemetry.ModelsBuilderMode, ModelsBuilderMode),
            new(Constants.Telemetry.RuntimeMode, RuntimeMode),
            new(Constants.Telemetry.AspEnvironment, AspEnvironment), new(Constants.Telemetry.IsDebug, IsDebug),
            new(Constants.Telemetry.DatabaseProvider, DatabaseProvider),
            new(Constants.Telemetry.CurrentServerRole, CurrentServerRole),
        };

    /// <inheritdoc />
    public IDictionary<string, string> GetTroubleshootingInformation() =>
        new Dictionary<string, string>
        {
            { "Server OS", ServerOs },
            { "Server Framework", ServerFramework },
            { "Default Language", _localizationService.GetDefaultLanguageIsoCode() },
            { "Umbraco Version", _version.SemanticVersion.ToSemanticStringWithoutBuild() },
            { "Current Culture", CurrentCulture },
            { "Current UI Culture", Thread.CurrentThread.CurrentUICulture.ToString() },
            { "Current Webserver", CurrentWebServer },
            { "Models Builder Mode", ModelsBuilderMode },
            { "Runtime Mode", RuntimeMode },
            { "Debug Mode", IsDebug.ToString() },
            { "Database Provider", DatabaseProvider },
            { "Current Server Role", CurrentServerRole },
        };

    private string GetWebServerName()
    {
        var processName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);

        if (processName.Contains("w3wp"))
        {
            return "IIS";
        }

        if (processName.Contains("iisexpress"))
        {
            return "IIS Express";
        }

        return "Kestrel";
    }
}
