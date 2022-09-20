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

internal class SystemInformationTelemetryProvider : IDetailedTelemetryProvider, IUserDataService
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly HostingSettings _hostingSettings;
    private readonly ILocalizationService _localizationService;
    private readonly ModelsBuilderSettings _modelsBuilderSettings;
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private readonly IUmbracoVersion _version;
    private readonly IServerRoleAccessor _serverRoleAccessor;

    [Obsolete($"Use the constructor that does not take an IOptionsMonitor<GlobalSettings> parameter, scheduled for removal in V12")]
    public SystemInformationTelemetryProvider(
        IUmbracoVersion version,
        ILocalizationService localizationService,
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IOptionsMonitor<HostingSettings> hostingSettings,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IHostEnvironment hostEnvironment,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IServerRoleAccessor serverRoleAccessor)
        : this(version, localizationService, modelsBuilderSettings, hostingSettings, hostEnvironment, umbracoDatabaseFactory, serverRoleAccessor)
    {
    }

    public SystemInformationTelemetryProvider(
        IUmbracoVersion version,
        ILocalizationService localizationService,
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IOptionsMonitor<HostingSettings> hostingSettings,
        IHostEnvironment hostEnvironment,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IServerRoleAccessor serverRoleAccessor)
    {
        _version = version;
        _localizationService = localizationService;
        _hostEnvironment = hostEnvironment;
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _serverRoleAccessor = serverRoleAccessor;

        _hostingSettings = hostingSettings.CurrentValue;
        _modelsBuilderSettings = modelsBuilderSettings.CurrentValue;
    }

    private string CurrentWebServer => IsRunningInProcessIIS() ? "IIS" : "Kestrel";

    private string ServerFramework => RuntimeInformation.FrameworkDescription;

    private string ModelsBuilderMode => _modelsBuilderSettings.ModelsMode.ToString();

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
            new(Constants.Telemetry.AspEnvironment, AspEnvironment), new(Constants.Telemetry.IsDebug, IsDebug),
            new(Constants.Telemetry.DatabaseProvider, DatabaseProvider),
            new(Constants.Telemetry.CurrentServerRole, CurrentServerRole),
        };

    public IEnumerable<UserData> GetUserData() =>
        new UserData[]
        {
            new("Server OS", ServerOs), new("Server Framework", ServerFramework),
            new("Default Language", _localizationService.GetDefaultLanguageIsoCode()),
            new("Umbraco Version", _version.SemanticVersion.ToSemanticStringWithoutBuild()),
            new("Current Culture", CurrentCulture),
            new("Current UI Culture", Thread.CurrentThread.CurrentUICulture.ToString()),
            new("Current Webserver", CurrentWebServer), new("Models Builder Mode", ModelsBuilderMode),
            new("Debug Mode", IsDebug.ToString()), new("Database Provider", DatabaseProvider),
            new("Current Server Role", CurrentServerRole),
        };

    private bool IsRunningInProcessIIS()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        var processName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
        return processName.Contains("w3wp") || processName.Contains("iisexpress");
    }
}
