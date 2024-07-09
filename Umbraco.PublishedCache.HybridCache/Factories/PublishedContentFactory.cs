using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.Factories;

internal class PublishedContentFactory : IPublishedContentFactory
{
    private readonly PublishedContentTypeCache _contentTypeCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedModelFactory _publishedModelFactory;


    public PublishedContentFactory(
        IVariationContextAccessor variationContextAccessor,
        IPublishedModelFactory publishedModelFactory,
        IContentTypeService contentTypeService,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        ILoggerFactory loggerFactory)
    {
        _variationContextAccessor = variationContextAccessor;
        _publishedModelFactory = publishedModelFactory;
        _contentTypeCache = new PublishedContentTypeCache(
            contentTypeService,
            null,
            null,
            publishedContentTypeFactory,
            loggerFactory.CreateLogger<PublishedContentTypeCache>());
    }

    public IPublishedContent? ToIPublishedContent(ContentCacheNode contentCacheNode, bool preview)
    {
        var n = new ContentNode(contentCacheNode.Id, contentCacheNode.Key, contentCacheNode.Path, contentCacheNode.SortOrder, contentCacheNode.CreateDate, contentCacheNode.CreatorId);
        IPublishedContentType contentType = _contentTypeCache.Get(PublishedItemType.Content, contentCacheNode.ContentTypeId);
        n.SetContentTypeAndData(contentType, contentCacheNode.Draft, contentCacheNode.Published, _variationContextAccessor, _publishedModelFactory);
        return preview ? n.DraftModel : n.PublishedModel;
    }
}
