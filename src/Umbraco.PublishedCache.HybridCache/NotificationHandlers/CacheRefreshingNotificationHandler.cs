using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

/// <summary>
///     Handles content and media cache invalidation in response to content, media, and type change notifications.
/// </summary>
/// <remarks>
///     For structural content/media type changes, the rebuild can run immediately (default) or be deferred
///     to a background task via <see cref="DeferredCacheRebuildNotificationHandler" /> when
///     <see cref="CacheSettings.ContentTypeRebuildMode" /> is set to <see cref="ContentTypeRebuildMode.Deferred" />.
/// </remarks>
#pragma warning disable CS0618 // Type or member is obsolete
internal sealed class CacheRefreshingNotificationHandler :
    INotificationAsyncHandler<ContentRefreshNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>,
    INotificationAsyncHandler<MediaRefreshNotification>,
    INotificationAsyncHandler<MediaDeletedNotification>,
    INotificationAsyncHandler<ElementRefreshNotification>,
    INotificationAsyncHandler<ElementDeletedNotification>,
    INotificationAsyncHandler<ContentTypeRefreshedNotification>,
    INotificationAsyncHandler<ContentTypeDeletedNotification>,
    INotificationAsyncHandler<MediaTypeRefreshedNotification>,
    INotificationAsyncHandler<MediaTypeDeletedNotification>
#pragma warning restore CS0618 // Type or member is obsolete
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IElementCacheService _elementCacheService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<CacheRefreshingNotificationHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CacheRefreshingNotificationHandler" /> class.
    /// </summary>
    /// <param name="documentCacheService">The document cache service.</param>
    /// <param name="mediaCacheService">The media cache service.</param>
    /// <param name="elementCacheService">The element cache service.</param>
    /// <param name="publishedContentTypeCache">The published content type cache.</param>
    /// <param name="cacheSettings">The cache settings.</param>
    /// <param name="logger">The logger.</param>
    public CacheRefreshingNotificationHandler(
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService,
        IElementCacheService elementCacheService,
        IPublishedContentTypeCache publishedContentTypeCache,
        IOptions<CacheSettings> cacheSettings,
        ILogger<CacheRefreshingNotificationHandler> logger)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _elementCacheService = elementCacheService;
        _publishedContentTypeCache = publishedContentTypeCache;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public async Task HandleAsync(ContentRefreshNotification notification, CancellationToken cancellationToken)
#pragma warning restore CS0618 // Type or member is obsolete
        => await _documentCacheService.RefreshContentAsync(notification.Entity);

    /// <inheritdoc />
    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContent deletedEntity in notification.DeletedEntities)
        {
            await _documentCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public async Task HandleAsync(MediaRefreshNotification notification, CancellationToken cancellationToken)
#pragma warning restore CS0618 // Type or member is obsolete
        => await _mediaCacheService.RefreshMediaAsync(notification.Entity);

    /// <inheritdoc />
    public async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMedia deletedEntity in notification.DeletedEntities)
        {
            await _mediaCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    /// <inheritdoc />
    public async Task HandleAsync(ElementRefreshNotification notification, CancellationToken cancellationToken)
        => await _elementCacheService.RefreshElementAsync(notification.Entity);

    /// <inheritdoc />
    public async Task HandleAsync(ElementDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IElement deletedEntity in notification.DeletedEntities)
        {
            await _elementCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public Task HandleAsync(ContentTypeRefreshedNotification notification, CancellationToken cancellationToken)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        // Split changes into those that need a full raw cmsContentNu rebuild and those that only need the
        // converted content cache cleared (ContentCacheNode only stores ContentTypeId). A structural change
        // flagged RawDataUnaffected (a property removal) keeps its stored blob valid, so it belongs with the
        // converted-clear group. Content type changes can affect both documents and elements, so route by IsElement.
        var documentRebuild = new HashSet<int>();
        var documentConvertedOnly = new HashSet<int>();
        var elementRebuild = new HashSet<int>();
        var elementConvertedOnly = new HashSet<int>();

        foreach (ContentTypeChange<IContentType> change in notification.Changes)
        {
            var id = change.Item.Id;
            var isElement = change.Item.IsElement;

            if (change.ChangeTypes.RequiresRawDataRebuild())
            {
                (isElement ? elementRebuild : documentRebuild).Add(id);
            }
            else if (change.ChangeTypes.RequiresConvertedCacheClearOnly())
            {
                (isElement ? elementConvertedOnly : documentConvertedOnly).Add(id);
            }
        }

        RefreshCacheForContentTypeChanges(documentRebuild, documentConvertedOnly, _documentCacheService.Rebuild, _documentCacheService.ClearConvertedContentCache);
        RefreshCacheForContentTypeChanges(elementRebuild, elementConvertedOnly, _elementCacheService.Rebuild, _elementCacheService.ClearConvertedContentCache);

        return Task.CompletedTask;
    }

    private void RefreshCacheForContentTypeChanges(
        HashSet<int> rebuildIds,
        HashSet<int> clearConvertedOnlyIds,
        Action<IReadOnlyCollection<int>> rebuild,
        Action<IReadOnlyCollection<int>> clearConvertedCache)
    {
        // Clear content type definitions for all affected types
        foreach (var contentTypeId in rebuildIds.Concat(clearConvertedOnlyIds))
        {
            _publishedContentTypeCache.ClearContentType(contentTypeId);
        }

        // Full rebuild only for structural changes that affect the stored data (alias changed, variation
        // changed, etc.). In deferred mode, the rebuild is queued from the ContentTypeChangedNotification
        // handler instead, which fires after the scope is disposed — avoiding database lock contention between
        // the deferred rebuild's transaction and the original save transaction.
        if (rebuildIds.Count > 0)
        {
            if (_cacheSettings.ContentTypeRebuildMode != ContentTypeRebuildMode.Deferred)
            {
                rebuild(rebuildIds.ToArray());
            }
            else
            {
                _logger.LogDebug(
                    "Content type change: database cache rebuild for {Count} type(s) left to the deferred rebuild (ContentTypeRebuildMode.Deferred).",
                    rebuildIds.Count);
            }
        }

        // For changes that leave the stored data valid (name, icon, description, new property added, or a
        // property removal flagged RawDataUnaffected), just clear the converted content cache - HybridCache
        // entries remain valid. Selective clearing is safe here because no model factory reset occurs in this handler.
        if (clearConvertedOnlyIds.Count > 0)
        {
            clearConvertedCache(clearConvertedOnlyIds.ToArray());
        }
    }

    /// <inheritdoc />
    public Task HandleAsync(ContentTypeDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContentType deleted in notification.DeletedEntities)
        {
            _publishedContentTypeCache.ClearContentType(deleted.Id);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public Task HandleAsync(MediaTypeRefreshedNotification notification, CancellationToken cancellationToken)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        // Split changes into those that need a full raw cmsContentNu rebuild and those that only need the
        // converted content cache cleared (ContentCacheNode only stores ContentTypeId). A structural change
        // flagged RawDataUnaffected (a property removal) keeps its stored blob valid, so it belongs with the
        // converted-clear group.
        var rebuildIds = new HashSet<int>();
        var clearConvertedOnlyIds = new HashSet<int>();

        foreach (ContentTypeChange<IMediaType> change in notification.Changes)
        {
            if (change.ChangeTypes.RequiresRawDataRebuild())
            {
                rebuildIds.Add(change.Item.Id);
            }
            else if (change.ChangeTypes.RequiresConvertedCacheClearOnly())
            {
                clearConvertedOnlyIds.Add(change.Item.Id);
            }
        }

        RefreshCacheForContentTypeChanges(rebuildIds, clearConvertedOnlyIds, _mediaCacheService.Rebuild, _mediaCacheService.ClearConvertedContentCache);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandleAsync(MediaTypeDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMediaType deleted in notification.DeletedEntities)
        {
            _publishedContentTypeCache.ClearContentType(deleted.Id);
        }

        return Task.CompletedTask;
    }
}
