using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal class SeedingNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IContentCacheService _contentCacheService;
    private readonly CacheSettings _cacheSettings;

    public SeedingNotificationHandler(IContentCacheService contentCacheService, IOptions<CacheSettings> cacheSettings)
    {
        _contentCacheService = contentCacheService;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken) => await _contentCacheService.SeedAsync(_cacheSettings.ContentTypeIds);
}
