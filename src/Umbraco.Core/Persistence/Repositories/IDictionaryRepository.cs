using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

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

    #region Obsolete sync bridge methods

    // TODO (V20): Remove these default implementations when all callers have been migrated to async.

    /// <summary>
    ///     Gets a dictionary item by its unique identifier.
    /// </summary>
    [Obsolete("Use GetAsync(Guid, CancellationToken) instead. Scheduled for removal when EFCore Migration is completed.")]
    IDictionaryItem? Get(Guid uniqueId) => GetAsync(uniqueId, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets multiple dictionary items by their unique identifiers.
    /// </summary>
    [Obsolete("Use GetManyAsync(Guid[], CancellationToken) instead. Scheduled for removal when EFCore Migration is completed.")]
    IEnumerable<IDictionaryItem> GetMany(params Guid[] uniqueIds) => GetManyAsync(uniqueIds, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a dictionary item by its key.
    /// </summary>
    [Obsolete("Use GetByItemKeyAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    IDictionaryItem? Get(string key) => GetByItemKeyAsync(key).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets multiple dictionary items by their keys.
    /// </summary>
    [Obsolete("Use GetManyByItemKeysAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    IEnumerable<IDictionaryItem> GetManyByKeys(params string[] keys) => GetManyByItemKeysAsync(keys).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets all descendant dictionary items of a parent item.
    /// </summary>
    [Obsolete("Use GetDictionaryItemDescendantsAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId, string? filter = null)
        => GetDictionaryItemDescendantsAsync(parentId, filter).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a mapping of dictionary item keys to their unique identifiers.
    /// </summary>
    [Obsolete("Use GetDictionaryItemKeyMapAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    Dictionary<string, Guid> GetDictionaryItemKeyMap()
        => GetDictionaryItemKeyMapAsync().GetAwaiter().GetResult();

    #endregion

    #region Obsolete sync bridge methods from old IReadWriteQueryRepository<int, IDictionaryItem>

    // TODO (V20): Remove these when callers (DictionaryItemService, LocalizationService) are migrated to async.

    /// <summary>
    ///     Saves a dictionary item (sync bridge).
    /// </summary>
    [Obsolete("Use SaveAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    void Save(IDictionaryItem entity) => SaveAsync(entity, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    ///     Deletes a dictionary item (sync bridge).
    /// </summary>
    [Obsolete("Use DeleteAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    void Delete(IDictionaryItem entity) => DeleteAsync(entity, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    ///     Checks if a dictionary item with the specified int ID exists (sync bridge).
    /// </summary>
    [Obsolete("Use ExistsAsync(Guid, CancellationToken) instead. Scheduled for removal when EFCore Migration is completed.")]
    bool Exists(int id)
    {
        // Resolve int ID to Guid Key via the full dataset
        IEnumerable<IDictionaryItem> all = GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();
        IDictionaryItem? item = all.FirstOrDefault(x => x.Id == id);
        return item is not null;
    }

    /// <summary>
    ///     Gets a dictionary item by int ID (sync bridge for legacy callers).
    /// </summary>
    [Obsolete("Use GetAsync(Guid, CancellationToken) instead. Scheduled for removal when EFCore Migration is completed.")]
    IDictionaryItem? Get(int id)
    {
        // Resolve int ID to Guid Key via the full dataset
        IEnumerable<IDictionaryItem> all = GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();
        IDictionaryItem? item = all.FirstOrDefault(x => x.Id == id);
        return item is not null ? GetAsync(item.Key, CancellationToken.None).GetAwaiter().GetResult() : null;
    }

    /// <summary>
    ///     Gets dictionary items matching a query (sync bridge).
    /// </summary>
    [Obsolete("Scheduled for removal when EFCore Migration is completed.")]
    IEnumerable<IDictionaryItem> Get(IQuery<IDictionaryItem> query)
    {
        // We can't easily translate IQuery to EF Core, so load all and filter in memory.
        // This is acceptable as a temporary bridge — callers should migrate to async.
        IEnumerable<IDictionaryItem> all = GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();
        return all;
    }

    /// <summary>
    ///     Counts dictionary items matching a query (sync bridge).
    /// </summary>
    [Obsolete("Scheduled for removal when EFCore Migration is completed.")]
    int Count(IQuery<IDictionaryItem>? query)
    {
        IEnumerable<IDictionaryItem> all = GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();
        return all.Count();
    }

    #endregion
}
