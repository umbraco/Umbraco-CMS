using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal sealed class CacheRefreshingNotificationHandler : INotificationAsyncHandler<ContentRefreshNotification>, INotificationAsyncHandler<ContentDeletedNotification>
{
    private readonly IContentCacheService _contentCacheService;

    public CacheRefreshingNotificationHandler(IContentCacheService contentCacheService) => _contentCacheService = contentCacheService;

    public async Task HandleAsync(ContentRefreshNotification notification, CancellationToken cancellationToken) => await _contentCacheService.RefreshContent(notification.Entity);

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContent deletedEntity in notification.DeletedEntities)
        {
            await _contentCacheService.DeleteItem(deletedEntity.Id);
        }
    }
}
