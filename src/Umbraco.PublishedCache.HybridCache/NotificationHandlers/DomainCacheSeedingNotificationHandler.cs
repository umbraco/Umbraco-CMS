using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal sealed class DomainCacheSeedingNotificationHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly IDomainCacheService _domainCacheService;
    private readonly IRuntimeState _runtimeState;
    private readonly GlobalSettings _globalSettings;

    public DomainCacheSeedingNotificationHandler(IDomainCacheService domainCacheService, IRuntimeState runtimeState, IOptions<GlobalSettings> globalSettings)
    {
        _domainCacheService = domainCacheService;
        _runtimeState = runtimeState;
        _globalSettings = globalSettings.Value;
    }

    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        if (_runtimeState.Level <= RuntimeLevel.Install || (_runtimeState.Level == RuntimeLevel.Upgrade && _globalSettings.ShowMaintenancePageWhenInUpgradeState))
        {
            return;
        }

        _domainCacheService.GetAll(includeWildcards: true);
    }
}
