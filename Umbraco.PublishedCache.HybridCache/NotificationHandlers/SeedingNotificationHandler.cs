using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal class SeedingNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IContentCacheService _contentCacheService;
    private readonly IServerRoleAccessor _serverRoleAccessor;

    public SeedingNotificationHandler(IContentCacheService contentCacheService, IServerRoleAccessor serverRoleAccessor)
    {
        _contentCacheService = contentCacheService;
        _serverRoleAccessor = serverRoleAccessor;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        if (_serverRoleAccessor.CurrentServerRole is ServerRole.Subscriber or ServerRole.Unknown)
        {
            return;
        }

        await _contentCacheService.SeedAsync(new List<int>());
    }
}
