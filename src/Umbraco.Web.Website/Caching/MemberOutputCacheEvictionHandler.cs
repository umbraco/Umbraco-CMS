using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Handles <see cref="MemberCacheRefresherNotification"/> to evict output cache entries
///     for pages that reference the changed member via picker properties.
/// </summary>
internal sealed class MemberOutputCacheEvictionHandler : INotificationAsyncHandler<MemberCacheRefresherNotification>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRelationService _relationService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ILogger<MemberOutputCacheEvictionHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberOutputCacheEvictionHandler"/> class.
    /// </summary>
    public MemberOutputCacheEvictionHandler(
        IServiceProvider serviceProvider,
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        ILogger<MemberOutputCacheEvictionHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _relationService = relationService;
        _idKeyMap = idKeyMap;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(MemberCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        IOutputCacheStore? store = _serviceProvider.GetService<IOutputCacheStore>();
        if (store is null)
        {
            return;
        }

        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not MemberCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        foreach (MemberCacheRefresher.JsonPayload payload in payloads)
        {
            await EvictRelatedPagesAsync(store, payload.Id, cancellationToken);
        }
    }

    private async Task EvictRelatedPagesAsync(IOutputCacheStore store, int memberId, CancellationToken cancellationToken)
    {
        IEnumerable<IRelation> relations = _relationService.GetByChildId(memberId, Constants.Conventions.RelationTypes.RelatedMemberAlias);
        foreach (IRelation relation in relations)
        {
            Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(relation.ParentId, UmbracoObjectTypes.Document);
            if (parentKeyAttempt.Success)
            {
                _logger.LogDebug(
                    "Member {MemberId} is referenced by {ParentKey} — evicting.",
                    memberId,
                    parentKeyAttempt.Result);
                await store.EvictByTagAsync(Constants.Website.OutputCache.ContentTagPrefix + parentKeyAttempt.Result, cancellationToken);
            }
        }
    }
}
