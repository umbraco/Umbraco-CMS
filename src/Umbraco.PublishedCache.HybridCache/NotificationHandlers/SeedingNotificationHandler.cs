using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal class SeedingNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;

    public SeedingNotificationHandler(IDocumentCacheService documentCacheService, IMediaCacheService mediaCacheService)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification,
        CancellationToken cancellationToken)
    {
        // TODO: Investigate if these can run in parallel
        await _documentCacheService.SeedAsync();
        await _mediaCacheService.SeedAsync();
    }
}
