using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

/// <summary>
///     Rebuilds the database cache if required when the serializer changes
/// </summary>
public class HybridCacheStartupNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private readonly IDatabaseCacheRebuilder _databaseCacheRebuilder;
    private readonly IRuntimeState _runtimeState;

    public HybridCacheStartupNotificationHandler(IDatabaseCacheRebuilder databaseCacheRebuilder, IRuntimeState runtimeState)
    {
        _databaseCacheRebuilder = databaseCacheRebuilder;
        _runtimeState = runtimeState;
    }

    public Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level > RuntimeLevel.Install)
        {
            _databaseCacheRebuilder.RebuildDatabaseCacheIfSerializerChanged();
        }

        return Task.CompletedTask;
    }
}
