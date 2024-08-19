using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal sealed class CacheRefreshingNotificationHandler :
    INotificationAsyncHandler<ContentRefreshNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>,
    INotificationAsyncHandler<MediaRefreshNotification>,
    INotificationAsyncHandler<MediaDeletedNotification>
{
    private readonly IContentCacheService _contentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    public CacheRefreshingNotificationHandler(IContentCacheService contentCacheService, IMediaCacheService mediaCacheService, IPublishedSnapshotAccessor publishedSnapshotAccessor)
    {
        _contentCacheService = contentCacheService;
        _mediaCacheService = mediaCacheService;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
    }

    public async Task HandleAsync(ContentRefreshNotification notification, CancellationToken cancellationToken)
    {
        await _contentCacheService.RefreshContentAsync(notification.Entity);

        if (_publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? snapshot))
        {
            snapshot.ElementsCache?.Clear();
        }
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContent deletedEntity in notification.DeletedEntities)
        {
            if (_publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? snapshot))
            {
                snapshot.ElementsCache?.Clear();
            }

            await _contentCacheService.DeleteItemAsync(deletedEntity.Id);
        }
    }

    public async Task HandleAsync(MediaRefreshNotification notification, CancellationToken cancellationToken) => await _mediaCacheService.RefreshMediaAsync(notification.Entity);

    public async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMedia deletedEntity in notification.DeletedEntities)
        {
            await _mediaCacheService.DeleteItemAsync(deletedEntity.Id);
        }
    }
}
