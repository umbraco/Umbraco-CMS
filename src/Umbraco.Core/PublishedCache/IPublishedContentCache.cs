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

    /// <summary>
    ///     Gets several content items identified by their unique identifiers.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="keys">The unique identifiers of the content items.</param>
    /// <returns>
    ///     The content items found, in the order of <paramref name="keys" />. A key that resolves to no content is omitted.
    /// </returns>
    /// <remarks>
    ///     Implementations should read whatever is not cached in one batch, not once per key. The default implementation
    ///     reads one key at a time.
    /// </remarks>
    // TODO (V20): Remove the default implementation.
    IReadOnlyList<IPublishedContent> GetByIds(bool preview, IReadOnlyCollection<Guid> keys)
    {
        var content = new List<IPublishedContent>(keys.Count);
        foreach (Guid key in keys)
        {
            IPublishedContent? item = GetById(preview, key);
            if (item is not null)
            {
                content.Add(item);
            }
        }

        return content;
    }
}
