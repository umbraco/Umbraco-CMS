using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal class SeedingNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly CacheSettings _cacheSettings;

    public SeedingNotificationHandler(IDocumentCacheService documentCacheService, IOptions<CacheSettings> cacheSettings)
    {
        _documentCacheService = documentCacheService;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken) => await _documentCacheService.SeedAsync(_cacheSettings.ContentTypeKeys);
}
