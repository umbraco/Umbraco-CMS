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
    INotificationAsyncHandler<ElementRefreshNotification>,
    INotificationAsyncHandler<ElementDeletedNotification>,
    INotificationAsyncHandler<ContentTypeRefreshedNotification>,
    INotificationAsyncHandler<ContentTypeDeletedNotification>,
    INotificationAsyncHandler<MediaTypeRefreshedNotification>,
    INotificationAsyncHandler<MediaTypeDeletedNotification>
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IElementCacheService _elementCacheService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;

    public CacheRefreshingNotificationHandler(
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService,
        IElementCacheService elementCacheService,
        IPublishedContentTypeCache publishedContentTypeCache)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _elementCacheService = elementCacheService;
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

    public async Task HandleAsync(ElementRefreshNotification notification, CancellationToken cancellationToken)
        => await _elementCacheService.RefreshElementAsync(notification.Entity);

    public async Task HandleAsync(ElementDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IElement deletedEntity in notification.DeletedEntities)
        {
            await _elementCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    public Task HandleAsync(ContentTypeRefreshedNotification notification, CancellationToken cancellationToken)
    {
        // Separate structural changes (RefreshMain) from non-structural changes (RefreshOther).
        // Structural changes require a full rebuild, while non-structural changes only need
        // the converted content cache cleared since ContentCacheNode only stores ContentTypeId.
        // Content type changes can affect both documents and elements, so route based on IsElement.
        var documentStructural = new HashSet<int>();
        var documentNonStructural = new HashSet<int>();
        var elementStructural = new HashSet<int>();
        var elementNonStructural = new HashSet<int>();

        foreach (ContentTypeChange<IContentType> change in notification.Changes)
        {
            var id = change.Item.Id;
            var isElement = change.Item.IsElement;

            if (change.ChangeTypes.IsStructuralChange())
            {
                (isElement ? elementStructural : documentStructural).Add(id);
            }
            else if (change.ChangeTypes.IsNonStructuralChange())
            {
                (isElement ? elementNonStructural : documentNonStructural).Add(id);
            }
        }

        RefreshCacheForContentTypeChanges(documentStructural, documentNonStructural, _documentCacheService.Rebuild, _documentCacheService.ClearConvertedContentCache);
        RefreshCacheForContentTypeChanges(elementStructural, elementNonStructural, _elementCacheService.Rebuild, _elementCacheService.ClearConvertedContentCache);

        return Task.CompletedTask;
    }

    private void RefreshCacheForContentTypeChanges(
        HashSet<int> structuralChangeIds,
        HashSet<int> nonStructuralChangeIds,
        Action<IReadOnlyCollection<int>> rebuild,
        Action<IReadOnlyCollection<int>> clearConvertedCache)
    {
        // Clear content type definitions for all affected types
        foreach (var contentTypeId in structuralChangeIds.Concat(nonStructuralChangeIds))
        {
            _publishedContentTypeCache.ClearContentType(contentTypeId);
        }

        // Full rebuild only for structural changes (property removed, alias changed, variation changed, etc.)
        if (structuralChangeIds.Count > 0)
        {
            rebuild(structuralChangeIds.ToArray());
        }

        // For non-structural changes (name, icon, description, new property added),
        // just clear the converted content cache - HybridCache entries remain valid.
        // Selective clearing is safe here because no model factory reset occurs in this handler.
        if (nonStructuralChangeIds.Count > 0)
        {
            clearConvertedCache(nonStructuralChangeIds.ToArray());
        }
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
        var structuralChangeIds = new HashSet<int>();
        var nonStructuralChangeIds = new HashSet<int>();

        foreach (ContentTypeChange<IMediaType> change in notification.Changes)
        {
            if (change.ChangeTypes.IsStructuralChange())
            {
                structuralChangeIds.Add(change.Item.Id);
            }
            else if (change.ChangeTypes.IsNonStructuralChange())
            {
                nonStructuralChangeIds.Add(change.Item.Id);
            }
        }

        RefreshCacheForContentTypeChanges(structuralChangeIds, nonStructuralChangeIds, _mediaCacheService.Rebuild, _mediaCacheService.ClearConvertedContentCache);
        return Task.CompletedTask;
    }

    public Task HandleAsync(MediaTypeDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMediaType deleted in notification.DeletedEntities)
        {
            _publishedContentTypeCache.ClearContentType(deleted.Id);
        }

        return Task.CompletedTask;
    }
}
