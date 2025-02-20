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
    private readonly IElementsCache _elementsCache;
    private readonly IRelationService _relationService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;

    public CacheRefreshingNotificationHandler(
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService,
        IElementsCache elementsCache,
        IRelationService relationService,
        IPublishedContentTypeCache publishedContentTypeCache)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _elementsCache = elementsCache;
        _relationService = relationService;
        _publishedContentTypeCache = publishedContentTypeCache;
    }

    public async Task HandleAsync(ContentRefreshNotification notification, CancellationToken cancellationToken)
    {
        ClearElementsCache();

        await _documentCacheService.RefreshContentAsync(notification.Entity);
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContent deletedEntity in notification.DeletedEntities)
        {
            ClearElementsCache();
            await _documentCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    public async Task HandleAsync(MediaRefreshNotification notification, CancellationToken cancellationToken)
    {
        ClearElementsCache();
        await _mediaCacheService.RefreshMediaAsync(notification.Entity);
    }

    public async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMedia deletedEntity in notification.DeletedEntities)
        {
            ClearElementsCache();
            await _mediaCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    private void ClearElementsCache()
    {
        // Ideally we'd like to not have to clear the entire cache here. However, this was the existing behavior in NuCache.
        // The reason for this is that we have no way to know which elements are affected by the changes. or what their keys are.
        // This is because currently published elements lives exclusively in a JSON blob in the umbracoPropertyData table.
        // This means that the only way to resolve these keys are to actually parse this data with a specific value converter, and for all cultures, which is not feasible.
        // If published elements become their own entities with relations, instead of just property data, we can revisit this,
        _elementsCache.Clear();
    }

    public Task HandleAsync(ContentTypeRefreshedNotification notification, CancellationToken cancellationToken)
    {
        const ContentTypeChangeTypes types // only for those that have been refreshed
            = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
        var contentTypeIds = notification.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id)
            .ToArray();

        if (contentTypeIds.Length > 0)
        {
            foreach (var contentTypeId in contentTypeIds)
            {
                _publishedContentTypeCache.ClearContentType(contentTypeId);
            }

            _documentCacheService.Rebuild(contentTypeIds);
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
        const ContentTypeChangeTypes types // only for those that have been refreshed
            = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
        var mediaTypeIds = notification.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id)
            .ToArray();

        if (mediaTypeIds.Length > 0)
        {
            foreach (var mediaTypeId in mediaTypeIds)
            {
                _publishedContentTypeCache.ClearContentType(mediaTypeId);
            }

            _mediaCacheService.Rebuild(mediaTypeIds);
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
