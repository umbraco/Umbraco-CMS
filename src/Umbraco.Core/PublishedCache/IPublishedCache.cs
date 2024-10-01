using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
///     Provides access to cached contents.
/// </summary>
public interface IPublishedCache
{
    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="contentId">The content unique identifier.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>The value of <paramref name="preview" /> overrides defaults.</remarks>
    IPublishedContent? GetById(bool preview, int contentId);

    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="contentId">The content unique identifier.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>The value of <paramref name="preview" /> overrides defaults.</remarks>
    IPublishedContent? GetById(bool preview, Guid contentId);

    /// <summary>
    ///     Gets a content identified by its Udi identifier.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="contentId">The content Udi identifier.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>The value of <paramref name="preview" /> overrides defaults.</remarks>
    [Obsolete] // FIXME: Remove when replacing nucache
    IPublishedContent? GetById(bool preview, Udi contentId);

    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="contentId">The content unique identifier.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    IPublishedContent? GetById(int contentId);

    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="contentId">The content unique identifier.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    IPublishedContent? GetById(Guid contentId);

    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="contentId">The content unique identifier.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    [Obsolete] // FIXME: Remove when replacing nucache
    IPublishedContent? GetById(Udi contentId);

    /// <summary>
    ///     Gets contents at root.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="culture">A culture.</param>
    /// <returns>The contents.</returns>
    /// <remarks>The value of <paramref name="preview" /> overrides defaults.</remarks>
    [Obsolete("Scheduled for removal, use IDocumentNavigationQueryService instead in v17")]
    IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null);

    /// <summary>
    ///     Gets contents at root.
    /// </summary>
    /// <param name="culture">A culture.</param>
    /// <returns>The contents.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    [Obsolete("Scheduled for removal, use IDocumentNavigationQueryService instead in v17")]
    IEnumerable<IPublishedContent> GetAtRoot(string? culture = null);

    /// <summary>
    ///     Gets a value indicating whether the cache contains published content.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <returns>A value indicating whether the cache contains published content.</returns>
    /// <remarks>The value of <paramref name="preview" /> overrides defaults.</remarks>
    [Obsolete("Scheduled for removal in v17")]
    bool HasContent(bool preview);

    /// <summary>
    ///     Gets a value indicating whether the cache contains published content.
    /// </summary>
    /// <returns>A value indicating whether the cache contains published content.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    [Obsolete("Scheduled for removal in v17")]
    bool HasContent();

    /// <summary>
    ///     Gets a content type identified by its unique identifier.
    /// </summary>
    /// <param name="id">The content type unique identifier.</param>
    /// <returns>The content type, or null.</returns>
    [Obsolete("Please use the IContentTypeCacheService instead, scheduled for removal in V16")]
    IPublishedContentType? GetContentType(int id);

    /// <summary>
    ///     Gets a content type identified by its alias.
    /// </summary>
    /// <param name="alias">The content type alias.</param>
    /// <returns>The content type, or null.</returns>
    /// <remarks>The alias is case-insensitive.</remarks>
    [Obsolete("Please use the IContentTypeCacheService instead, scheduled for removal in V16")]
    IPublishedContentType? GetContentType(string alias);

    /// <summary>
    ///     Gets contents of a given content type.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>The contents.</returns>
    [Obsolete]
    IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType);

    /// <summary>
    ///     Gets a content type identified by its alias.
    /// </summary>
    /// <param name="key">The content type key.</param>
    /// <returns>The content type, or null.</returns>
    [Obsolete("Please use the IContentTypeCacheService instead, scheduled for removal in V16")]
    IPublishedContentType? GetContentType(Guid key);
}
