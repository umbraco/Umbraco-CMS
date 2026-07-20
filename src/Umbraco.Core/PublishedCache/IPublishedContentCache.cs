using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Provides access to cached published content items with async support.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IPublishedCache"/> with asynchronous methods
/// for retrieving content items, supporting both integer IDs and GUIDs.
/// </remarks>
public interface IPublishedContentCache : IPublishedCache
{
    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="id">The content unique identifier.</param>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null);

    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="key">The content unique identifier.</param>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    Task<IPublishedContent?> GetByIdAsync(Guid key, bool? preview = null);
}
