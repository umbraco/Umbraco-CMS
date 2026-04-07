using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IDictionaryItem" /> entities.
/// </summary>
public interface IDictionaryRepository : IAsyncReadWriteRepository<Guid, IDictionaryItem>
{
    /// <summary>
    ///     Gets a dictionary item by its string key.
    /// </summary>
    /// <param name="key">The string key of the dictionary item.</param>
    /// <returns>The dictionary item if found; otherwise, <c>null</c>.</returns>
    Task<IDictionaryItem?> GetByItemKeyAsync(string key);

    /// <summary>
    ///     Gets multiple dictionary items by their string keys.
    /// </summary>
    /// <param name="keys">The string keys of the dictionary items.</param>
    /// <returns>A collection of dictionary items.</returns>
    Task<IEnumerable<IDictionaryItem>> GetManyByItemKeysAsync(params string[] keys);

    /// <summary>
    ///     Gets all descendant dictionary items of a parent item.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent item, or <c>null</c> for root items.</param>
    /// <param name="filter">An optional filter to apply to the results.</param>
    /// <returns>A collection of descendant dictionary items.</returns>
    Task<IEnumerable<IDictionaryItem>> GetDictionaryItemDescendantsAsync(Guid? parentId, string? filter = null);

    /// <summary>
    ///     Gets a mapping of dictionary item keys to their unique identifiers.
    /// </summary>
    /// <returns>A dictionary mapping keys to unique identifiers.</returns>
    Task<Dictionary<string, Guid>> GetDictionaryItemKeyMapAsync();

    /// <summary>
    ///     Gets a dictionary item by int ID (sync bridge for legacy callers).
    /// </summary>
    [Obsolete("Use GetAsync(Guid, CancellationToken) instead. Scheduled for removal in Umbraco 18.")]
    IDictionaryItem? Get(int id)
    {
        // Resolve int ID to Guid Key via the full dataset
        IEnumerable<IDictionaryItem> all = GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();
        IDictionaryItem? item = all.FirstOrDefault(x => x.Id == id);
        return item is not null ? GetAsync(item.Key, CancellationToken.None).GetAwaiter().GetResult() : null;
    }
}
