using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal class SeedingNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IRuntimeState _runtimeState;
    private readonly GlobalSettings _globalSettings;

    public SeedingNotificationHandler(IDocumentCacheService documentCacheService, IMediaCacheService mediaCacheService, IRuntimeState runtimeState, IOptions<GlobalSettings> globalSettings)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _runtimeState = runtimeState;
        _globalSettings = globalSettings.Value;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification,
        CancellationToken cancellationToken)
    {

        if (_runtimeState.Level <= RuntimeLevel.Install || (_runtimeState.Level == RuntimeLevel.Upgrade && _globalSettings.ShowMaintenancePageWhenInUpgradeState))
        {
            return;
        }

        await _documentCacheService.SeedAsync(cancellationToken);
        await _mediaCacheService.SeedAsync(cancellationToken);
    }
}
