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

    /// <summary>
    /// Synchronously gets a published element by its unique key.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="key">The element's unique key.</param>
    /// <returns>The published element, or <c>null</c> if not found.</returns>
    /// <remarks>
    /// Implementations should serve the warm-cache case without setting up an async state
    /// machine. The default implementation blocks on <see cref="GetByIdAsync(Guid, bool?)"/>
    /// for backwards compatibility; consumers calling sync-over-async should prefer this
    /// overload.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    IPublishedElement? GetById(bool preview, Guid key)
        => GetByIdAsync(key, preview).GetAwaiter().GetResult();
}
