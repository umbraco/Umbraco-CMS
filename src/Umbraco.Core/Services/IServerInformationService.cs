using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides server information about the current Umbraco instance.
/// </summary>
public interface IServerInformationService
{
    /// <summary>
    ///     Gets information about the server including version, timezone, and runtime mode.
    /// </summary>
    /// <returns>A <see cref="ServerInformation"/> object containing the server details.</returns>
    ServerInformation GetServerInformation();
}

/// <summary>
///     Default implementation of <see cref="IServerInformationService"/>.
/// </summary>
public class ServerInformationService : IServerInformationService
{
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly TimeProvider _timeProvider;
    private RuntimeSettings _runtimeSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerInformationService"/> class.
    /// </summary>
    /// <param name="umbracoVersion">The Umbraco version provider.</param>
    /// <param name="timeProvider">The time provider for timezone information.</param>
    /// <param name="runtimeSettingsOptionsMonitor">The runtime settings monitor.</param>
    public ServerInformationService(IUmbracoVersion umbracoVersion, TimeProvider timeProvider, IOptionsMonitor<RuntimeSettings> runtimeSettingsOptionsMonitor)
    {
        _umbracoVersion = umbracoVersion;
        _timeProvider = timeProvider;
        _runtimeSettings = runtimeSettingsOptionsMonitor.CurrentValue;
        runtimeSettingsOptionsMonitor.OnChange(runtimeSettings => _runtimeSettings = runtimeSettings);
    }

    /// <inheritdoc />
    public ServerInformation GetServerInformation() => new(_umbracoVersion.SemanticVersion, _timeProvider.LocalTimeZone, _runtimeSettings.Mode);
}
