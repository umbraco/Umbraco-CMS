using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Provides access to cached published elements.
/// </summary>
public interface IPublishedElementCache
{
    /// <summary>
    /// Gets a published element by its unique key.
    /// </summary>
    /// <param name="key">The element's unique key.</param>
    /// <param name="preview">Optional value indicating whether to include unpublished content.</param>
    /// <returns>The published element, or <c>null</c> if not found.</returns>
    Task<IPublishedElement?> GetByIdAsync(Guid key, bool? preview = null);
}
