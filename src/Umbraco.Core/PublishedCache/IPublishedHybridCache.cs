using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IPublishedHybridCache
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
    ///     Gets a value indicating whether the cache contains a specified content.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="contentId">The content unique identifier.</param>
    /// <returns>A value indicating whether to the cache contains the specified content.</returns>
    /// <remarks>The value of <paramref name="preview" /> overrides defaults.</remarks>
    bool HasById(bool preview, int contentId);

    /// <summary>
    ///     Gets a value indicating whether the cache contains a specified content.
    /// </summary>
    /// <param name="contentId">The content unique identifier.</param>
    /// <returns>A value indicating whether to the cache contains the specified content.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    bool HasById(int contentId);

    /// <summary>
    ///     Gets a value indicating whether the cache contains published content.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <returns>A value indicating whether the cache contains published content.</returns>
    /// <remarks>The value of <paramref name="preview" /> overrides defaults.</remarks>
    bool HasContent(bool preview);

    /// <summary>
    ///     Gets a value indicating whether the cache contains published content.
    /// </summary>
    /// <returns>A value indicating whether the cache contains published content.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    bool HasContent();
}
