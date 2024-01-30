using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IServerInformationService
{
    ServerInformation GetServerInformation();
}

public class ServerInformationService : IServerInformationService
{
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly TimeProvider _timeProvider;
    private RuntimeSettings _runtimeSettings;

    public ServerInformationService(IUmbracoVersion umbracoVersion, TimeProvider timeProvider, IOptionsMonitor<RuntimeSettings> runtimeSettingsOptionsMonitor)
    {
        _umbracoVersion = umbracoVersion;
        _timeProvider = timeProvider;
        _runtimeSettings = runtimeSettingsOptionsMonitor.CurrentValue;
        runtimeSettingsOptionsMonitor.OnChange(runtimeSettings => _runtimeSettings = runtimeSettings);
    }

    public ServerInformation GetServerInformation() => new(_umbracoVersion.SemanticVersion, _timeProvider.LocalTimeZone, _runtimeSettings.Mode);
}
