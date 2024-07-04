using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal sealed class CacheRefreshingNotificationHandler : INotificationAsyncHandler<ContentRefreshNotification>
{
    private readonly ICacheService _cacheService;

    public CacheRefreshingNotificationHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task HandleAsync(ContentRefreshNotification notification, CancellationToken cancellationToken) => await _cacheService.RefreshContent(notification.Entity);
}
