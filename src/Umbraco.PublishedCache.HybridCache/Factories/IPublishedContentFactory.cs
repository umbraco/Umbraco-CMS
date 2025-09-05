using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Factories;

/// <summary>
/// Defines a factory to create <see cref="IPublishedContent"/> and <see cref="IPublishedMember"/> from a <see cref="ContentCacheNode"/> or <see cref="IMember"/>.
/// </summary>
internal interface IPublishedContentFactory
{
    /// <summary>
    /// Converts a <see cref="ContentCacheNode"/> to an <see cref="IPublishedContent"/> if document type.
    /// </summary>
    IPublishedContent? ToIPublishedContent(ContentCacheNode contentCacheNode, bool preview);

    /// <summary>
    /// Converts a <see cref="ContentCacheNode"/> to an <see cref="IPublishedContent"/> of media type.
    /// </summary>
    IPublishedContent? ToIPublishedMedia(ContentCacheNode contentCacheNode);

    /// <summary>
    /// Converts a <see cref="IMember"/> to an <see cref="IPublishedMember"/>.
    /// </summary>
    IPublishedMember ToPublishedMember(IMember member);

    IPublishedElement? ToIPublishedElement(ContentCacheNode contentCacheNode, bool preview);
}
