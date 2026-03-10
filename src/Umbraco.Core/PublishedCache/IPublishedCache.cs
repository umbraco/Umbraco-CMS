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
}
