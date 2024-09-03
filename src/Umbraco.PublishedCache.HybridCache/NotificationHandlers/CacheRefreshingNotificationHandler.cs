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
    INotificationAsyncHandler<ContentTypeRefreshedNotification>
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
            await RefreshElementsCacheAsync(deletedEntity);
            await _documentCacheService.DeleteItemAsync(deletedEntity.Id);
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
            await RefreshElementsCacheAsync(deletedEntity);
            await _mediaCacheService.DeleteItemAsync(deletedEntity.Id);
        }
    }

    private async Task RefreshElementsCacheAsync(IUmbracoEntity content)
    {
        IEnumerable<IRelation> parentRelations = _relationService.GetByParent(content)!;
        IEnumerable<IRelation> childRelations = _relationService.GetByChild(content);

        var ids = parentRelations.Select(x => x.ChildId).Concat(childRelations.Select(x => x.ParentId)).ToHashSet();
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
                if (property.ReferenceCacheLevel != PropertyCacheLevel.Elements)
                {
                    continue;
                }

                _elementsCache.ClearByKey(property.ValuesCacheKey);
            }
        }
    }

    public Task HandleAsync(ContentTypeRefreshedNotification notification, CancellationToken cancellationToken)
    {
        const ContentTypeChangeTypes types // only for those that have been refreshed
            = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
        var contentTypeIds = notification.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id)
            .ToArray();

        if (contentTypeIds.Length != 0)
        {
            foreach (var contentTypeId in contentTypeIds)
            {
                _publishedContentTypeCache.ClearContentType(contentTypeId);
            }

            _documentCacheService.Rebuild(contentTypeIds);
        }

        return Task.CompletedTask;
    }
}
