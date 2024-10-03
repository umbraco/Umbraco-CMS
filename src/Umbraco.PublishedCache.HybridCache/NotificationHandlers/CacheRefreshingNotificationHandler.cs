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
        await RefreshElementsCacheAsync(notification.Entity);

        await _documentCacheService.RefreshContentAsync(notification.Entity);
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContent deletedEntity in notification.DeletedEntities)
        {
            RemoveFromElementsCache(deletedEntity);
            await _documentCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    public async Task HandleAsync(MediaRefreshNotification notification, CancellationToken cancellationToken)
    {
        await RefreshElementsCacheAsync(notification.Entity);
        await _mediaCacheService.RefreshMediaAsync(notification.Entity);
    }

    public async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMedia deletedEntity in notification.DeletedEntities)
        {
            RemoveFromElementsCache(deletedEntity);
            await _mediaCacheService.DeleteItemAsync(deletedEntity);
        }
    }

    private async Task RefreshElementsCacheAsync(IUmbracoEntity content)
    {
        IEnumerable<IRelation> parentRelations = _relationService.GetByParent(content)!;
        IEnumerable<IRelation> childRelations = _relationService.GetByChild(content);

        var ids = parentRelations.Select(x => x.ChildId).Concat(childRelations.Select(x => x.ParentId)).ToHashSet();
        // We need to add ourselves to the list of ids to clear
        ids.Add(content.Id);
        foreach (var id in ids)
        {
            if (await _documentCacheService.HasContentByIdAsync(id) is false)
            {
                continue;
            }

            IPublishedContent? publishedContent = await _documentCacheService.GetByIdAsync(id);
            if (publishedContent is null)
            {
                continue;
            }

            foreach (IPublishedProperty publishedProperty in publishedContent.Properties)
            {
                var property = (PublishedProperty) publishedProperty;
                if (property.ReferenceCacheLevel is PropertyCacheLevel.Elements
                    || property.PropertyType.DeliveryApiCacheLevel is PropertyCacheLevel.Elements
                    || property.PropertyType.DeliveryApiCacheLevelForExpansion is PropertyCacheLevel.Elements)
                {
                    _elementsCache.ClearByKey(property.ValuesCacheKey);
                }
            }
        }
    }

    private void RemoveFromElementsCache(IUmbracoEntity content)
    {
        // ClearByKey clears by "startsWith" so we'll clear by the cachekey prefix + contentKey
        // This will clear any and all properties for this content item, this is important because
        // we cannot resolve the PublishedContent for this entity since it and its content type is deleted.
        _elementsCache.ClearByKey(GetContentWideCacheKey(content.Key, true));
        _elementsCache.ClearByKey(GetContentWideCacheKey(content.Key, false));
    }

    private string GetContentWideCacheKey(Guid contentKey, bool isPreviewing) => isPreviewing
        ? CacheKeys.PreviewPropertyCacheKeyPrefix + contentKey
        : CacheKeys.PropertyCacheKeyPrefix + contentKey;

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
