using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal class SeedingNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IContentCacheService _contentCacheService;

    public SeedingNotificationHandler(IContentCacheService contentCacheService)
    {
        _contentCacheService = contentCacheService;
    }
    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        await _contentCacheService.SeedAsync(new List<int>());
    }
}
