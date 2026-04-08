using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Handles <see cref="MediaCacheRefresherNotification"/> to evict output cache entries
///     for pages that reference the changed media via picker properties.
/// </summary>
internal sealed class MediaOutputCacheEvictionHandler : INotificationAsyncHandler<MediaCacheRefresherNotification>
{
    private readonly IOutputCacheStore _outputCacheStore;
    private readonly IRelationService _relationService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ILogger<MediaOutputCacheEvictionHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaOutputCacheEvictionHandler"/> class.
    /// </summary>
    public MediaOutputCacheEvictionHandler(
        IOutputCacheStore outputCacheStore,
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        ILogger<MediaOutputCacheEvictionHandler> logger)
    {
        _outputCacheStore = outputCacheStore;
        _relationService = relationService;
        _idKeyMap = idKeyMap;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(MediaCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not MediaCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        foreach (MediaCacheRefresher.JsonPayload payload in payloads)
        {
            if (payload.ChangeTypes.HasFlag(TreeChangeTypes.RefreshAll))
            {
                _logger.LogDebug("Media refresh all — evicting all website output cache entries.");
                await _outputCacheStore.EvictByTagAsync(Constants.Website.OutputCache.AllContentTag, cancellationToken);
                return;
            }

            await EvictRelatedPagesAsync(payload.Id, payload.Key, cancellationToken);
        }
    }

    private async Task EvictRelatedPagesAsync(int mediaId, Guid? mediaKey, CancellationToken cancellationToken)
    {
        IEnumerable<IRelation> relations = _relationService.GetByChildId(mediaId, Constants.Conventions.RelationTypes.RelatedMediaAlias);
        foreach (IRelation relation in relations)
        {
            Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(relation.ParentId, UmbracoObjectTypes.Document);
            if (parentKeyAttempt.Success)
            {
                _logger.LogDebug(
                    "Media {MediaKey} is referenced by {ParentKey} — evicting.",
                    mediaKey,
                    parentKeyAttempt.Result);
                await _outputCacheStore.EvictByTagAsync(Constants.Website.OutputCache.ContentTagPrefix + parentKeyAttempt.Result, cancellationToken);
            }
        }
    }
}
