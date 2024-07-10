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


    public PublishedContentFactory(
        IVariationContextAccessor variationContextAccessor,
        IContentTypeService contentTypeService,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        ILoggerFactory loggerFactory)
    {
        _variationContextAccessor = variationContextAccessor;
        _contentTypeCache = new PublishedContentTypeCache(
            contentTypeService,
            null,
            null,
            publishedContentTypeFactory,
            loggerFactory.CreateLogger<PublishedContentTypeCache>());
    }

    public IPublishedContent? ToIPublishedContent(ContentCacheNode contentCacheNode, bool preview)
    {
        var contentNode = new ContentNode(contentCacheNode.Id, contentCacheNode.Key, contentCacheNode.Path, contentCacheNode.SortOrder, contentCacheNode.CreateDate, contentCacheNode.CreatorId);
        IPublishedContentType contentType = _contentTypeCache.Get(PublishedItemType.Content, contentCacheNode.ContentTypeId);
        contentNode.SetContentTypeAndData(contentType, contentCacheNode.Draft, contentCacheNode.Published);

        return preview ? GetModel(contentNode, contentNode.DraftModel) ?? GetPublishedContentAsDraft(GetModel(contentNode, contentNode.PublishedModel)) : GetModel(contentNode, contentNode.PublishedModel);
    }

    private IPublishedContent? GetModel(ContentNode node, ContentData? contentData) =>
        contentData == null
            ? null
            : new PublishedContent(
                node,
                contentData,
                _variationContextAccessor);

    private IPublishedContent? GetPublishedContentAsDraft(IPublishedContent? content) =>
        content == null ? null :
            // an object in the cache is either an IPublishedContentOrMedia,
            // or a model inheriting from PublishedContentExtended - in which
            // case we need to unwrap to get to the original IPublishedContentOrMedia.
            UnwrapIPublishedContent(content);

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
