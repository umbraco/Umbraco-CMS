using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PublishedCache.Persistence;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

/// <summary>
///     Rebuilds the database cache if required when the serializer changes
/// </summary>
public class NuCacheStartupHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly INuCacheContentService _nuCacheContentService;
    private readonly IRuntimeState _runtimeState;

    public NuCacheStartupHandler(
        INuCacheContentService nuCacheContentService,
        IRuntimeState runtimeState)
    {
        _nuCacheContentService = nuCacheContentService;
        _runtimeState = runtimeState;
    }

    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        if (_runtimeState.Level == RuntimeLevel.Run)
        {
            _nuCacheContentService.RebuildDatabaseCacheIfSerializerChanged();
        }
    }
}
