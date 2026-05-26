using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
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
    INotificationAsyncHandler<ContentTypeRefreshedNotification>,
    INotificationAsyncHandler<ContentTypeDeletedNotification>,
    INotificationAsyncHandler<MediaTypeRefreshedNotification>,
    INotificationAsyncHandler<MediaTypeDeletedNotification>
#pragma warning restore CS0618 // Type or member is obsolete
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly CacheSettings _cacheSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CacheRefreshingNotificationHandler" /> class.
    /// </summary>
    /// <param name="documentCacheService">The document cache service.</param>
    /// <param name="mediaCacheService">The media cache service.</param>
    /// <param name="publishedContentTypeCache">The published content type cache.</param>
    /// <param name="cacheSettings">The cache settings.</param>
    public CacheRefreshingNotificationHandler(
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService,
        IPublishedContentTypeCache publishedContentTypeCache,
        IOptions<CacheSettings> cacheSettings)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _publishedContentTypeCache = publishedContentTypeCache;
        _cacheSettings = cacheSettings.Value;
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
#pragma warning disable CS0618 // Type or member is obsolete
    public Task HandleAsync(ContentTypeRefreshedNotification notification, CancellationToken cancellationToken)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        // Separate structural changes (RefreshMain) from non-structural changes (RefreshOther).
        // Structural changes require a full rebuild, while non-structural changes only need
        // the converted content cache cleared since ContentCacheNode only stores ContentTypeId.
        var structuralChangeIds = notification.Changes
            .Where(x => x.ChangeTypes.IsStructuralChange())
            .Select(x => x.Item.Id)
            .ToArray();

        var nonStructuralChangeIds = notification.Changes
            .Where(x => x.ChangeTypes.IsNonStructuralChange())
            .Select(x => x.Item.Id)
            .ToArray();

        // Clear content type cache for all changes - content type definitions need refreshing regardless
        foreach (var contentTypeId in structuralChangeIds.Concat(nonStructuralChangeIds))
        {
            _publishedContentTypeCache.ClearContentType(contentTypeId);
        }

        // Full rebuild only for structural changes (property removed, alias changed, variation changed, etc.)
        // In deferred mode, the rebuild is queued from the ContentTypeChangedNotification handler instead,
        // which fires after the scope is disposed — avoiding database lock contention between the
        // deferred rebuild's transaction and the original save transaction.
        if (structuralChangeIds.Length > 0 && _cacheSettings.ContentTypeRebuildMode != ContentTypeRebuildMode.Deferred)
        {
            _documentCacheService.Rebuild(structuralChangeIds);
        }

        // For non-structural changes (name, icon, description, new property added),
        // just clear the converted content cache - HybridCache entries remain valid.
        // Selective clearing is safe here because no model factory reset occurs in this handler.
        if (nonStructuralChangeIds.Length > 0)
        {
            _documentCacheService.ClearConvertedContentCache(nonStructuralChangeIds);
        }

        return Task.CompletedTask;
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
        // Separate structural changes (RefreshMain) from non-structural changes (RefreshOther).
        // Structural changes require a full rebuild, while non-structural changes only need
        // the converted content cache cleared since ContentCacheNode only stores ContentTypeId.
        var structuralChangeIds = notification.Changes
            .Where(x => x.ChangeTypes.IsStructuralChange())
            .Select(x => x.Item.Id)
            .ToArray();

        var nonStructuralChangeIds = notification.Changes
            .Where(x => x.ChangeTypes.IsNonStructuralChange())
            .Select(x => x.Item.Id)
            .ToArray();

        // Clear media type cache for all changes - media type definitions need refreshing regardless
        foreach (var mediaTypeId in structuralChangeIds.Concat(nonStructuralChangeIds))
        {
            _publishedContentTypeCache.ClearContentType(mediaTypeId);
        }

        // Full rebuild only for structural changes (property removed, alias changed, variation changed, etc.)
        // In deferred mode, the rebuild is queued from the MediaTypeChangedNotification handler instead,
        // which fires after the scope is disposed — avoiding database lock contention between the
        // deferred rebuild's transaction and the original save transaction.
        if (structuralChangeIds.Length > 0 && _cacheSettings.ContentTypeRebuildMode != ContentTypeRebuildMode.Deferred)
        {
            _mediaCacheService.Rebuild(structuralChangeIds);
        }

        // For non-structural changes (name, icon, description, new property added),
        // just clear the converted content cache - HybridCache entries remain valid.
        // Selective clearing is safe here because no model factory reset occurs in this handler.
        if (nonStructuralChangeIds.Length > 0)
        {
            _mediaCacheService.ClearConvertedContentCache(nonStructuralChangeIds);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandleAsync(MediaTypeDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMediaType deleted in notification.DeletedEntities )
        {
            _publishedContentTypeCache.ClearContentType(deleted.Id);
        }

        return Task.CompletedTask;
    }
}
