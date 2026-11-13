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
    IPublishedElement? GetById(bool preview, Guid key);

    /// <summary>
    /// Gets multiple published elements by their unique keys.
    /// </summary>
    /// <param name="keys">The elements' unique keys.</param>
    /// <param name="preview">Optional value indicating whether to include unpublished content.</param>
    /// <returns>The published elements that exist, in the same order as <paramref name="keys"/> (missing elements omitted).</returns>
    /// <remarks>
    /// Implementations should load the elements that are not cached in a single read rather than one at a time.
    /// The default implementation retrieves each element with <see cref="GetByIdAsync(Guid, bool?)"/>.
    /// </remarks>
    // TODO (V20): Remove the default implementation.
    async Task<IReadOnlyList<IPublishedElement>> GetByIdsAsync(IReadOnlyCollection<Guid> keys, bool? preview = null)
    {
        var result = new List<IPublishedElement>(keys.Count);
        foreach (Guid key in keys)
        {
            IPublishedElement? element = await GetByIdAsync(key, preview);
            if (element is not null)
            {
                result.Add(element);
            }
        }

        return result;
    }

    /// <summary>
    /// Synchronously gets multiple published elements by their unique keys.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="keys">The elements' unique keys.</param>
    /// <returns>The published elements that exist, in the same order as <paramref name="keys"/> (missing elements omitted).</returns>
    /// <remarks>
    /// Implementations should load the elements that are not cached in a single read rather than one at a time.
    /// The default implementation retrieves each element with <see cref="GetById(bool, Guid)"/>.
    /// </remarks>
    // TODO (V20): Remove the default implementation.
    IReadOnlyList<IPublishedElement> GetByIds(bool preview, IReadOnlyCollection<Guid> keys)
    {
        var result = new List<IPublishedElement>(keys.Count);
        foreach (Guid key in keys)
        {
            IPublishedElement? element = GetById(preview, key);
            if (element is not null)
            {
                result.Add(element);
            }
        }

        return result;
    }
}
