using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IPublishedMediaCache : IPublishedCache
{
    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="id">The content unique identifier.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    Task<IPublishedContent?> GetByIdAsync(int id);

    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="key">The content unique identifier.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    Task<IPublishedContent?> GetByKeyAsync(Guid key);

    /// <summary>
    ///     Gets a value indicating whether the cache contains a specified content.
    /// </summary>
    /// <param name="id">The content unique identifier.</param>
    /// <returns>A value indicating whether to the cache contains the specified content.</returns>
    Task<bool> HasByIdAsync(int id);
}
