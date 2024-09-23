using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal class SeedingNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IRuntimeState _runtimeState;

    public SeedingNotificationHandler(IDocumentCacheService documentCacheService, IMediaCacheService mediaCacheService, IRuntimeState runtimeState)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _runtimeState = runtimeState;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification,
        CancellationToken cancellationToken)
    {

        if (_runtimeState.Level <= RuntimeLevel.Install)
        {
            return;
        }

        await Task.WhenAll(
            _documentCacheService.SeedAsync(cancellationToken),
            _mediaCacheService.SeedAsync(cancellationToken)
        );
    }
}
