using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache.Factories;

/// <summary>
/// Defines a factory to create <see cref="IPublishedContent"/> and <see cref="IPublishedMember"/> from a <see cref="ContentCacheNode"/> or <see cref="IMember"/>.
/// </summary>
internal sealed class PublishedContentFactory : IPublishedContentFactory
{
    private readonly IElementsCache _elementsCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedContentFactory"/> class.
    /// </summary>
    public PublishedContentFactory(
        IElementsCache elementsCache,
        IVariationContextAccessor variationContextAccessor,
        IPublishedContentTypeCache publishedContentTypeCache)
    {
        _elementsCache = elementsCache;
        _variationContextAccessor = variationContextAccessor;
        _publishedContentTypeCache = publishedContentTypeCache;
    }

    /// <inheritdoc/>
    public IPublishedContent? ToIPublishedContent(ContentCacheNode contentCacheNode, bool preview)
    {
        ContentNode contentNode = CreateContentNode(contentCacheNode, preview);

        IPublishedContent? publishedContent = GetModel(contentNode, preview);

        if (preview)
        {
            publishedContent ??= GetPublishedContentAsDraft(publishedContent);
        }

        return publishedContent;
    }

    public IPublishedElement? ToIPublishedElement(ContentCacheNode contentCacheNode, bool preview)
    {
        ContentNode contentNode = CreateContentNode(contentCacheNode, preview);

        IPublishedElement? publishedElement = GetPublishedElement(contentNode, preview);

        if (preview)
        {
            // TODO ELEMENTS: what is the element equivalent of this?
            // return model ?? GetPublishedContentAsDraft(model);
        }

        return publishedElement;
    }

    /// <inheritdoc/>
    public IPublishedContent? ToIPublishedMedia(ContentCacheNode contentCacheNode)
    {
        IPublishedContentType contentType =
            _publishedContentTypeCache.Get(PublishedItemType.Media, contentCacheNode.ContentTypeId);
        var contentNode = new ContentNode(
            contentCacheNode.Id,
            contentCacheNode.Key,
            contentCacheNode.SortOrder,
            contentCacheNode.CreateDate.EnsureUtc(),
            contentCacheNode.CreatorId,
            contentType,
            null,
            contentCacheNode.Data);

        return GetModel(contentNode, false);
    }

    /// <inheritdoc/>
    public IPublishedMember ToPublishedMember(IMember member)
    {
        IPublishedContentType contentType =
            _publishedContentTypeCache.Get(PublishedItemType.Member, member.ContentTypeId);

        // Members are only "mapped" never cached, so these default values are a bit weird, but they are not used.
        var contentData = new ContentData(
            member.Name,
            null,
            0,
            member.UpdateDate.EnsureUtc(),
            member.CreatorId,
            null,
            true,
            GetPropertyValues(contentType, member),
            null);

        var contentNode = new ContentNode(
            member.Id,
            member.Key,
            member.SortOrder,
            member.UpdateDate.EnsureUtc(),
            member.CreatorId,
            contentType,
            null,
            contentData);
        return new PublishedMember(member, contentNode, _elementsCache, _variationContextAccessor);
    }

    private ContentNode CreateContentNode(ContentCacheNode contentCacheNode, bool preview)
    {
        IPublishedContentType contentType = _publishedContentTypeCache.Get(PublishedItemType.Content, contentCacheNode.ContentTypeId);
        return new ContentNode(
            contentCacheNode.Id,
            contentCacheNode.Key,
            contentCacheNode.SortOrder,
            contentCacheNode.CreateDate,
            contentCacheNode.CreatorId,
            contentType,
            preview ? contentCacheNode.Data : null,
            preview ? null : contentCacheNode.Data);
    }

    private static Dictionary<string, PropertyData[]> GetPropertyValues(IPublishedContentType contentType, IMember member)
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

    private static void AddIf(IPublishedContentType contentType, IDictionary<string, PropertyData[]> properties, string alias, object? value)
    {
        IPublishedPropertyType? propertyType = contentType.GetPropertyType(alias);
        if (propertyType == null || propertyType.IsUserProperty)
        {
            return;
        }

        properties[alias] = new[] { new PropertyData { Value = value, Culture = string.Empty, Segment = string.Empty } };
    }

    // TODO ELEMENTS: rename this to GetPublishedContent
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

    private IPublishedElement? GetPublishedElement(ContentNode node, bool preview)
    {
        ContentData? contentData = preview ? node.DraftModel : node.PublishedModel;
        return contentData == null
            ? null
            : new PublishedElement(
                node,
                preview,
                _elementsCache,
                _variationContextAccessor);
    }

    private static IPublishedContent? GetPublishedContentAsDraft(IPublishedContent? content) =>
        content == null ? null :
            // an object in the cache is either an IPublishedContentOrMedia,
            // or a model inheriting from PublishedContentExtended - in which
            // case we need to unwrap to get to the original IPublishedContentOrMedia.
            UnwrapIPublishedContent(content);

    private static PublishedContent UnwrapIPublishedContent(IPublishedContent content)
    {
        while (content is PublishedContentWrapped wrapped)
        {
            content = wrapped.Unwrap();
        }

        if (content is not PublishedContent inner)
        {
            throw new InvalidOperationException("Innermost content is not PublishedContent.");
        }

        return inner;
    }
}
