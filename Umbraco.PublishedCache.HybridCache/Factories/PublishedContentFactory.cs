using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

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

        return preview ? n.DraftModel ?? GetPublishedContentAsDraft(n.PublishedModel) : n.PublishedModel;
    }

    private IPublishedContent? GetPublishedContentAsDraft(IPublishedContent? content)
    {
        if (content == null)
        {
            return null;
        }

        // an object in the cache is either an IPublishedContentOrMedia,
        // or a model inheriting from PublishedContentExtended - in which
        // case we need to unwrap to get to the original IPublishedContentOrMedia.
        PublishedContent inner = UnwrapIPublishedContent(content);
        return inner.CreateModel(_publishedModelFactory);
    }

    private PublishedContent UnwrapIPublishedContent(IPublishedContent content)
    {
        while (content is PublishedContentWrapped wrapped)
        {
            content = wrapped.Unwrap();
        }

        if (!(content is PublishedContent inner))
        {
            throw new InvalidOperationException("Innermost content is not PublishedContent.");
        }

        return inner;
    }
}
