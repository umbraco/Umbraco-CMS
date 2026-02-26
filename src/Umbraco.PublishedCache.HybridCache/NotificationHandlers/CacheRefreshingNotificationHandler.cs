using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal sealed class CacheRefreshingNotificationHandler :
    INotificationAsyncHandler<ContentRefreshNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>,
    INotificationAsyncHandler<MediaRefreshNotification>,
    INotificationAsyncHandler<MediaDeletedNotification>,
    INotificationAsyncHandler<ContentTypeRefreshedNotification>,
    INotificationAsyncHandler<ContentTypeDeletedNotification>,
    INotificationAsyncHandler<MediaTypeRefreshedNotification>,
    INotificationAsyncHandler<MediaTypeDeletedNotification>
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;

    public CacheRefreshingNotificationHandler(
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService,
        IPublishedContentTypeCache publishedContentTypeCache)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _publishedContentTypeCache = publishedContentTypeCache;
    }

    public async Task HandleAsync(ContentRefreshNotification notification, CancellationToken cancellationToken)
        => await _documentCacheService.RefreshContentAsync(notification.Entity);

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContent deletedEntity in notification.DeletedEntities)
        {
            await _documentCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    public async Task HandleAsync(MediaRefreshNotification notification, CancellationToken cancellationToken)
        => await _mediaCacheService.RefreshMediaAsync(notification.Entity);

    public async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMedia deletedEntity in notification.DeletedEntities)
        {
            await _mediaCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    public Task HandleAsync(ContentTypeRefreshedNotification notification, CancellationToken cancellationToken)
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
        if (structuralChangeIds.Length > 0)
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

    public Task HandleAsync(ContentTypeDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContentType deleted in notification.DeletedEntities)
        {
            _publishedContentTypeCache.ClearContentType(deleted.Id);
        }

        return Task.CompletedTask;
    }

    public Task HandleAsync(MediaTypeRefreshedNotification notification, CancellationToken cancellationToken)
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
        if (structuralChangeIds.Length > 0)
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

    public Task HandleAsync(MediaTypeDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMediaType deleted in notification.DeletedEntities )
        {
            _publishedContentTypeCache.ClearContentType(deleted.Id);
        }

        return Task.CompletedTask;
    }
}
