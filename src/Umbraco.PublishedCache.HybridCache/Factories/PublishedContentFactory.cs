using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache.Factories;

internal class PublishedContentFactory : IPublishedContentFactory
{
    private readonly IElementsCache _elementsCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;


    public PublishedContentFactory(
        IElementsCache elementsCache,
        IVariationContextAccessor variationContextAccessor,
        IPublishedContentTypeCache publishedContentTypeCache)
    {
        _elementsCache = elementsCache;
        _variationContextAccessor = variationContextAccessor;
        _publishedContentTypeCache = publishedContentTypeCache;
    }

    public IPublishedContent? ToIPublishedContent(ContentCacheNode contentCacheNode, bool preview)
    {
        IPublishedContentType contentType = _publishedContentTypeCache.Get(PublishedItemType.Content, contentCacheNode.ContentTypeId);
        var contentNode = new ContentNode(
            contentCacheNode.Id,
            contentCacheNode.Key,
            contentCacheNode.SortOrder,
            contentCacheNode.CreateDate,
            contentCacheNode.CreatorId,
            contentType,
            preview ? contentCacheNode.Data : null,
            preview ? null : contentCacheNode.Data);

        IPublishedContent? model = GetModel(contentNode, preview);

        if (preview)
        {
            return model ?? GetPublishedContentAsDraft(model);
        }

        return model;
    }

    public IPublishedContent? ToIPublishedMedia(ContentCacheNode contentCacheNode)
    {
        IPublishedContentType contentType = _publishedContentTypeCache.Get(PublishedItemType.Media, contentCacheNode.ContentTypeId);
        var contentNode = new ContentNode(
            contentCacheNode.Id,
            contentCacheNode.Key,
            contentCacheNode.SortOrder,
            contentCacheNode.CreateDate,
            contentCacheNode.CreatorId,
            contentType,
            null,
            contentCacheNode.Data);

        return GetModel(contentNode, false);
    }

    public IPublishedMember ToPublishedMember(IMember member)
    {
        IPublishedContentType contentType = _publishedContentTypeCache.Get(PublishedItemType.Member, member.ContentTypeId);

        // Members are only "mapped" never cached, so these default values are a bit wierd, but they are not used.
        var contentData = new ContentData(
            member.Name,
            null,
            0,
            member.UpdateDate,
            member.CreatorId,
            null,
            true,
            GetPropertyValues(contentType, member),
            null);

        var contentNode = new ContentNode(
            member.Id,
            member.Key,
            member.SortOrder,
            member.UpdateDate,
            member.CreatorId,
            contentType,
            null,
            contentData);
        return new PublishedMember(member, contentNode, _elementsCache, _variationContextAccessor);
    }

    private Dictionary<string, PropertyData[]> GetPropertyValues(IPublishedContentType contentType, IMember member)
    {
        var properties = member
            .Properties
            .ToDictionary(
                x => x.Alias,
                x => new[] { new PropertyData { Value = x.GetValue(), Culture = string.Empty, Segment = string.Empty } },
                StringComparer.OrdinalIgnoreCase);

        // Add member properties
        AddIf(contentType, properties, nameof(IMember.Email), member.Email);
        AddIf(contentType, properties, nameof(IMember.Username), member.Username);
        AddIf(contentType, properties, nameof(IMember.Comments), member.Comments);
        AddIf(contentType, properties, nameof(IMember.IsApproved), member.IsApproved);
        AddIf(contentType, properties, nameof(IMember.IsLockedOut), member.IsLockedOut);
        AddIf(contentType, properties, nameof(IMember.LastLockoutDate), member.LastLockoutDate);
        AddIf(contentType, properties, nameof(IMember.CreateDate), member.CreateDate);
        AddIf(contentType, properties, nameof(IMember.LastLoginDate), member.LastLoginDate);
        AddIf(contentType, properties, nameof(IMember.LastPasswordChangeDate), member.LastPasswordChangeDate);

        return properties;
    }

    private void AddIf(IPublishedContentType contentType, IDictionary<string, PropertyData[]> properties, string alias, object? value)
    {
        IPublishedPropertyType? propertyType = contentType.GetPropertyType(alias);
        if (propertyType == null || propertyType.IsUserProperty)
        {
            return;
        }

        properties[alias] = new[] { new PropertyData { Value = value, Culture = string.Empty, Segment = string.Empty } };
    }

    private IPublishedContent? GetModel(ContentNode node, bool preview)
    {
        ContentData? contentData = preview ? node.DraftModel : node.PublishedModel;
        return contentData == null
            ? null
            : new PublishedContent(
                node,
                preview,
                _elementsCache,
                _variationContextAccessor);
    }


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
