using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Handles <see cref="ContentCacheRefresherNotification"/> to evict output cache entries
///     when content is published, unpublished, moved, or deleted. Also evicts pages that
///     reference the changed content via picker properties (umbDocument relations).
/// </summary>
internal sealed class DocumentOutputCacheEvictionHandler
    : RelationOutputCacheEvictionHandlerBase, INotificationAsyncHandler<ContentCacheRefresherNotification>
{
    private readonly IEnumerable<IWebsiteOutputCacheEvictionProvider> _evictionProviders;
    private readonly ILogger<DocumentOutputCacheEvictionHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentOutputCacheEvictionHandler"/> class.
    /// </summary>
    public DocumentOutputCacheEvictionHandler(
        IOutputCacheStore outputCacheStore,
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        IEnumerable<IWebsiteOutputCacheEvictionProvider> evictionProviders,
        ILogger<DocumentOutputCacheEvictionHandler> logger)
        : base(outputCacheStore, relationService, idKeyMap)
    {
        _evictionProviders = evictionProviders;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ContentCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not ContentCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        var changedEntityIds = new List<int>();

        foreach (ContentCacheRefresher.JsonPayload payload in payloads)
        {
            if (payload.Blueprint)
            {
                continue;
            }

            await EvictForPayloadAsync(payload, cancellationToken);
            changedEntityIds.Add(payload.Id);
        }

        // Evict documents that reference the changed content via picker properties.
        await EvictRelatedPagesAsync(
            changedEntityIds,
            Constants.Conventions.RelationTypes.RelatedDocumentAlias,
            _logger,
            cancellationToken);
    }

    private async Task EvictForPayloadAsync(ContentCacheRefresher.JsonPayload payload, CancellationToken cancellationToken)
    {
        if (payload.ChangeTypes.HasFlag(TreeChangeTypes.RefreshAll))
        {
            _logger.LogDebug("Evicting all website output cache entries.");
            await OutputCacheStore.EvictByTagAsync(Constants.Website.OutputCache.AllContentTag, cancellationToken);
            return;
        }

        if (payload.Key.HasValue)
        {
            Guid contentKey = payload.Key.Value;

            // Evict the specific content page.
            _logger.LogDebug("Evicting output cache for content {ContentKey}.", contentKey);
            await OutputCacheStore.EvictByTagAsync(Constants.Website.OutputCache.ContentTagPrefix + contentKey, cancellationToken);

            if (payload.ChangeTypes.HasFlag(TreeChangeTypes.RefreshBranch))
            {
                // Evict all descendants that tagged themselves with this ancestor.
                _logger.LogDebug("Evicting output cache for descendants of {ContentKey}.", contentKey);
                await OutputCacheStore.EvictByTagAsync(Constants.Website.OutputCache.AncestorTagPrefix + contentKey, cancellationToken);
            }

            // Invoke custom eviction providers.
            var context = new OutputCacheContentChangedContext(
                payload.Id,
                contentKey,
                payload.PublishedCultures ?? Array.Empty<string>(),
                payload.UnpublishedCultures ?? Array.Empty<string>());

            foreach (IWebsiteOutputCacheEvictionProvider provider in _evictionProviders)
            {
                IEnumerable<string> additionalTags = await provider.GetAdditionalEvictionTagsAsync(context, cancellationToken);
                foreach (var tag in additionalTags)
                {
                    _logger.LogDebug("Evicting output cache tag {Tag} via custom provider.", tag);
                    await OutputCacheStore.EvictByTagAsync(tag, cancellationToken);
                }
            }
        }
    }
}
