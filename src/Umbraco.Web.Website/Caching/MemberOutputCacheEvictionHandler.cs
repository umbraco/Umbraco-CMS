using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Handles <see cref="MemberCacheRefresherNotification"/> to evict output cache entries
///     for pages that reference the changed member via picker properties.
/// </summary>
internal sealed class MemberOutputCacheEvictionHandler
    : RelationOutputCacheEvictionHandlerBase, INotificationAsyncHandler<MemberCacheRefresherNotification>
{
    private readonly ILogger<MemberOutputCacheEvictionHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberOutputCacheEvictionHandler"/> class.
    /// </summary>
    public MemberOutputCacheEvictionHandler(
        IOutputCacheStore outputCacheStore,
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        ILogger<MemberOutputCacheEvictionHandler> logger)
        : base(outputCacheStore, relationService, idKeyMap)
        => _logger = logger;

    /// <inheritdoc />
    public async Task HandleAsync(MemberCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not MemberCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        // Evict documents that reference the changed members via picker properties.
        await EvictRelatedPagesAsync(
            payloads.Select(p => p.Id),
            Constants.Conventions.RelationTypes.RelatedMemberAlias,
            _logger,
            cancellationToken);
    }
}
