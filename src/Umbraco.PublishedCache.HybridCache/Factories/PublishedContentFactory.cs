using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Factories;

/// <summary>
/// Defines a factory to create <see cref="IPublishedContent"/> and <see cref="IPublishedMember"/> from a <see cref="ContentCacheNode"/> or <see cref="IMember"/>.
/// </summary>
internal sealed class PublishedContentFactory : IPublishedContentFactory
{
    private readonly IElementsCache _elementsCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly ILogger<PublishedContentFactory> _logger;
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedContentFactory"/> class.
    /// </summary>
    public PublishedContentFactory(
        IElementsCache elementsCache,
        IVariationContextAccessor variationContextAccessor,
        IPublishedContentTypeCache publishedContentTypeCache,
        ILogger<PublishedContentFactory> logger,
        AppCaches appCaches)
    {
        _elementsCache = elementsCache;
        _variationContextAccessor = variationContextAccessor;
        _publishedContentTypeCache = publishedContentTypeCache;
        _logger = logger;
        _appCaches = appCaches;
    }

    /// <inheritdoc/>
    public IPublishedContent? ToIPublishedContent(ContentCacheNode contentCacheNode, bool preview)
    {
        var cacheKey = $"{nameof(PublishedContentFactory)}DocumentCache_{contentCacheNode.Id}_{preview}";
        IPublishedContent? publishedContent = null;
        if (_appCaches.RequestCache.IsAvailable)
        {
            publishedContent = _appCaches.RequestCache.GetCacheItem<IPublishedContent?>(cacheKey);
            if (publishedContent is not null)
            {
                _logger.LogDebug(
                    "Using cached IPublishedContent for document {ContentCacheNodeName} ({ContentCacheNodeId}).",
                    contentCacheNode.Data?.Name ?? "No Name",
                    contentCacheNode.Id);
                return publishedContent;
            }
        }

        _logger.LogDebug(
            "Creating IPublishedContent for document {ContentCacheNodeName} ({ContentCacheNodeId}).",
            contentCacheNode.Data?.Name ?? "No Name",
            contentCacheNode.Id);

        IPublishedContentType contentType =
            _publishedContentTypeCache.Get(PublishedItemType.Content, contentCacheNode.ContentTypeId);
        var contentNode = new ContentNode(
            contentCacheNode.Id,
            contentCacheNode.Key,
            contentCacheNode.SortOrder,
            contentCacheNode.CreateDate,
            contentCacheNode.CreatorId,
            contentType,
            preview ? contentCacheNode.Data : null,
            preview ? null : contentCacheNode.Data);

        publishedContent = GetModel(contentNode, preview);

        if (preview)
        {
            publishedContent ??= GetPublishedContentAsDraft(publishedContent);
        }

        if (_appCaches.RequestCache.IsAvailable && publishedContent is not null)
        {
            _appCaches.RequestCache.Set(cacheKey, publishedContent);
        }

        return publishedContent;
    }

    /// <inheritdoc/>
    public IPublishedContent? ToIPublishedMedia(ContentCacheNode contentCacheNode)
    {
        var cacheKey = $"{nameof(PublishedContentFactory)}MediaCache_{contentCacheNode.Id}";
        IPublishedContent? publishedContent = null;
        if (_appCaches.RequestCache.IsAvailable)
        {
            publishedContent = _appCaches.RequestCache.GetCacheItem<IPublishedContent?>(cacheKey);
            if (publishedContent is not null)
            {
                _logger.LogDebug(
                    "Using cached IPublishedContent for media {ContentCacheNodeName} ({ContentCacheNodeId}).",
                    contentCacheNode.Data?.Name ?? "No Name",
                    contentCacheNode.Id);
                return publishedContent;
            }
        }

        _logger.LogDebug(
            "Creating IPublishedContent for media {ContentCacheNodeName} ({ContentCacheNodeId}).",
            contentCacheNode.Data?.Name ?? "No Name",
            contentCacheNode.Id);

        IPublishedContentType contentType =
            _publishedContentTypeCache.Get(PublishedItemType.Media, contentCacheNode.ContentTypeId);
        var contentNode = new ContentNode(
            contentCacheNode.Id,
            contentCacheNode.Key,
            contentCacheNode.SortOrder,
            contentCacheNode.CreateDate,
            contentCacheNode.CreatorId,
            contentType,
            null,
            contentCacheNode.Data);

        publishedContent = GetModel(contentNode, false);

        if (_appCaches.RequestCache.IsAvailable && publishedContent is not null)
        {
            _appCaches.RequestCache.Set(cacheKey, publishedContent);
        }

        return publishedContent;
    }

    /// <inheritdoc/>
    public IPublishedMember ToPublishedMember(IMember member)
    {
        string cacheKey = $"{nameof(PublishedContentFactory)}MemberCache_{member.Id}";
        IPublishedMember? publishedMember = null;
        if (_appCaches.RequestCache.IsAvailable)
        {
            publishedMember = _appCaches.RequestCache.GetCacheItem<IPublishedMember?>(cacheKey);
            if (publishedMember is not null)
            {
                _logger.LogDebug(
                    "Using cached IPublishedMember for member {MemberName} ({MemberId}).",
                    member.Username,
                    member.Id);

                return publishedMember;
            }
        }

        _logger.LogDebug(
            "Creating IPublishedMember for member {MemberName} ({MemberId}).",
            member.Username,
            member.Id);

        IPublishedContentType contentType =
            _publishedContentTypeCache.Get(PublishedItemType.Member, member.ContentTypeId);

        // Members are only "mapped" never cached, so these default values are a bit weird, but they are not used.
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
        publishedMember = new PublishedMember(member, contentNode, _elementsCache, _variationContextAccessor);

        if (_appCaches.RequestCache.IsAvailable)
        {
            _appCaches.RequestCache.Set(cacheKey, publishedMember);
        }

        return publishedMember;
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
