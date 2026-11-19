using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Provides access to cached published media items with async support.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IPublishedCache"/> with asynchronous methods
/// for retrieving media items, supporting both integer IDs and GUIDs.
/// </remarks>
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
    Task<IPublishedContent?> GetByIdAsync(Guid key);

    /// <summary>
    ///     Gets several media items identified by their unique identifiers.
    /// </summary>
    /// <param name="keys">The unique identifiers of the media items.</param>
    /// <returns>
    ///     The media items found, in the order of <paramref name="keys" />. A key that resolves to no media is omitted.
    /// </returns>
    /// <remarks>
    ///     Implementations should read whatever is not cached in one batch, not once per key. The default implementation
    ///     reads one key at a time.
    /// </remarks>
    // TODO (V20): Remove the default implementation.
    IReadOnlyList<IPublishedContent> GetByIds(IReadOnlyCollection<Guid> keys)
    {
        var media = new List<IPublishedContent>(keys.Count);
        foreach (Guid key in keys)
        {
            IPublishedContent? item = GetById(key);
            if (item is not null)
            {
                media.Add(item);
            }
        }

        return media;
    }
}
