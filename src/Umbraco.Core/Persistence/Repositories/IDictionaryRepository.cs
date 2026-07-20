using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IDictionaryItem" /> entities.
/// </summary>
public interface IDictionaryRepository : IReadWriteQueryRepository<int, IDictionaryItem>
{
    /// <summary>
    ///     Gets a dictionary item by its unique identifier.
    /// </summary>
    /// <param name="uniqueId">The unique identifier of the dictionary item.</param>
    /// <returns>The dictionary item if found; otherwise, <c>null</c>.</returns>
    IDictionaryItem? Get(Guid uniqueId);

    /// <summary>
    ///     Gets multiple dictionary items by their unique identifiers.
    /// </summary>
    /// <param name="uniqueIds">The unique identifiers of the dictionary items.</param>
    /// <returns>A collection of dictionary items.</returns>
    IEnumerable<IDictionaryItem> GetMany(params Guid[] uniqueIds) => Array.Empty<IDictionaryItem>();

    /// <summary>
    ///     Gets multiple dictionary items by their keys.
    /// </summary>
    /// <param name="keys">The keys of the dictionary items.</param>
    /// <returns>A collection of dictionary items.</returns>
    IEnumerable<IDictionaryItem> GetManyByKeys(params string[] keys) => Array.Empty<IDictionaryItem>();

    /// <summary>
    ///     Gets a dictionary item by its key.
    /// </summary>
    /// <param name="key">The key of the dictionary item.</param>
    /// <returns>The dictionary item if found; otherwise, <c>null</c>.</returns>
    IDictionaryItem? Get(string key);

    /// <summary>
    ///     Gets all descendant dictionary items of a parent item.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent item, or <c>null</c> for root items.</param>
    /// <param name="filter">An optional filter to apply to the results.</param>
    /// <returns>A collection of descendant dictionary items.</returns>
    IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId, string? filter = null);

    /// <summary>
    ///     Gets a mapping of dictionary item keys to their unique identifiers.
    /// </summary>
    /// <returns>A dictionary mapping keys to unique identifiers.</returns>
    Dictionary<string, Guid> GetDictionaryItemKeyMap();
}
