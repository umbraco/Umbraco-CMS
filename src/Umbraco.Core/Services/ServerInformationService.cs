using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Default implementation of <see cref="IServerInformationService"/>.
/// </summary>
public class ServerInformationService : IServerInformationService
{
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly TimeProvider _timeProvider;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IEnvironmentPermissions _environmentPermissions;
    private RuntimeSettings _runtimeSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerInformationService"/> class.
    /// </summary>
    /// <param name="umbracoVersion">The Umbraco version provider.</param>
    /// <param name="timeProvider">The time provider for timezone information.</param>
    /// <param name="runtimeSettingsOptionsMonitor">The runtime settings monitor.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ServerInformationService(IUmbracoVersion umbracoVersion, TimeProvider timeProvider, IOptionsMonitor<RuntimeSettings> runtimeSettingsOptionsMonitor)
        : this(
            umbracoVersion,
            timeProvider,
            runtimeSettingsOptionsMonitor,
            StaticServiceProvider.Instance.GetRequiredService<IHostingEnvironment>(),
            StaticServiceProvider.Instance.GetRequiredService<IEnvironmentPermissions>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerInformationService"/> class.
    /// </summary>
    /// <param name="umbracoVersion">The Umbraco version provider.</param>
    /// <param name="timeProvider">The time provider for timezone information.</param>
    /// <param name="runtimeSettingsOptionsMonitor">The runtime settings monitor.</param>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public ServerInformationService(
        IUmbracoVersion umbracoVersion,
        TimeProvider timeProvider,
        IOptionsMonitor<RuntimeSettings> runtimeSettingsOptionsMonitor,
        IHostingEnvironment hostingEnvironment)
        : this(
            umbracoVersion,
            timeProvider,
            runtimeSettingsOptionsMonitor,
            hostingEnvironment,
            StaticServiceProvider.Instance.GetRequiredService<IEnvironmentPermissions>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerInformationService"/> class.
    /// </summary>
    /// <param name="umbracoVersion">The Umbraco version provider.</param>
    /// <param name="timeProvider">The time provider for timezone information.</param>
    /// <param name="runtimeSettingsOptionsMonitor">The runtime settings monitor.</param>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    /// <param name="environmentPermissions">The permission verbs the environment denies.</param>
    public ServerInformationService(
        IUmbracoVersion umbracoVersion,
        TimeProvider timeProvider,
        IOptionsMonitor<RuntimeSettings> runtimeSettingsOptionsMonitor,
        IHostingEnvironment hostingEnvironment,
        IEnvironmentPermissions environmentPermissions)
    {
        _umbracoVersion = umbracoVersion;
        _timeProvider = timeProvider;
        _hostingEnvironment = hostingEnvironment;
        _environmentPermissions = environmentPermissions;
        _runtimeSettings = runtimeSettingsOptionsMonitor.CurrentValue;
        runtimeSettingsOptionsMonitor.OnChange(runtimeSettings => _runtimeSettings = runtimeSettings);
    }

    /// <inheritdoc />
    public ServerInformation GetServerInformation()
        => new(
            _umbracoVersion.SemanticVersion,
            _timeProvider.LocalTimeZone,
            _runtimeSettings.Mode,
            _hostingEnvironment.IsDebugMode,
            _environmentPermissions.DeniedPermissions.Order(StringComparer.Ordinal).ToArray());
}
