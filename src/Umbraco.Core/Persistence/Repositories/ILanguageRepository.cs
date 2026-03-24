using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="ILanguage" /> entities.
/// </summary>
public interface ILanguageRepository : IAsyncReadWriteRepository<Guid, ILanguage>
{
    /// <summary>
    ///     Gets a language by its ISO code.
    /// </summary>
    /// <param name="isoCode">The ISO code of the language.</param>
    /// <returns>The language if found; otherwise, <c>null</c>.</returns>
    Task<ILanguage?> GetByIsoCodeAsync(string isoCode);

    /// <summary>
    ///     Gets a language key from its ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    Task<Guid?> GetKeyByIsoCodeAsync(string? isoCode, bool throwOnNotFound = true);

    /// <summary>
    ///     Gets a language ISO code from its key.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    Task<string?> GetIsoCodeByKeyAsync(Guid? key, bool throwOnNotFound = true);

    /// <summary>
    ///     Gets the default language ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    Task<string> GetDefaultIsoCodeAsync();

    /// <summary>
    ///     Gets the default language.
    /// </summary>
    Task<ILanguage?> GetDefaultLanguageAsync();

    /// <summary>
    ///     Gets the default language key.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    Task<Guid?> GetDefaultKeyAsync();

    /// <summary>
    ///     Gets multiple language ISO codes from the provided Keys.
    /// </summary>
    /// <param name="keys">The language Keys.</param>
    /// <param name="throwOnNotFound">Indicates whether to throw an exception if the provided Key is not found as a language.</param>
    /// <returns></returns>
    Task<string[]> GetIsoCodesByKeysAsync(ICollection<Guid> keys, bool throwOnNotFound = true);

    #region Obsolete int-based methods (bridge for callers not yet migrated to Key)

    // TODO (V20): Remove these default implementations when all callers have migrated to Guid keys.
    // These bridge methods resolve int ID → Guid Key via the cached dataset, then delegate to the Key-based method.

    /// <summary>
    ///     Resolves an integer language ID to its Guid key. Returns null if not found.
    /// </summary>
    [Obsolete("Temporary bridge helper. Scheduled for removal when EFCore Migration is completed.")]
    private async Task<Guid?> ResolveKeyFromIdAsync(int id)
    {
        IEnumerable<ILanguage> all = await GetAllAsync(CancellationToken.None);
        return all.FirstOrDefault(x => x.Id == id)?.Key;
    }

    /// <summary>
    ///     Resolves a Guid language key to its integer ID. Returns null if not found.
    /// </summary>
    [Obsolete("Temporary bridge helper. Scheduled for removal when EFCore Migration is completed.")]
    private async Task<int?> ResolveIdFromKeyAsync(Guid key)
    {
        IEnumerable<ILanguage> all = await GetAllAsync(CancellationToken.None);
        return all.FirstOrDefault(x => x.Key == key)?.Id;
    }

    /// <summary>
    ///     Gets the default language integer ID.
    /// </summary>
    [Obsolete("Use GetDefaultKeyAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    async Task<int?> GetDefaultIdAsync()
    {
        Guid? key = await GetDefaultKeyAsync();
        return key.HasValue ? await ResolveIdFromKeyAsync(key.Value) : null;
    }

    /// <inheritdoc cref="IAsyncReadRepository{TKey, TEntity}.GetAsync"/>
    [Obsolete("Use GetAsync(Guid, CancellationToken) instead. Scheduled for removal when EFCore Migration is completed.")]
    async Task<ILanguage?> GetAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return null;
        }

        Guid? key = await ResolveKeyFromIdAsync(id.Value);
        return key.HasValue ? await GetAsync(key.Value, cancellationToken) : null;
    }

    /// <inheritdoc cref="IAsyncReadRepository{TKey, TEntity}.GetManyAsync"/>
    [Obsolete("Use GetManyAsync(Guid[], CancellationToken) instead. Scheduled for removal when EFCore Migration is completed.")]
    async Task<IEnumerable<ILanguage>> GetManyAsync(int[] ids, CancellationToken cancellationToken)
    {
        IEnumerable<ILanguage> all = await GetAllAsync(CancellationToken.None);
        var keys = all.Where(x => ids.Contains(x.Id)).Select(x => x.Key).ToArray();
        return await GetManyAsync(keys, cancellationToken);
    }

    /// <inheritdoc cref="IAsyncReadRepository{TKey, TEntity}.ExistsAsync"/>
    [Obsolete("Use ExistsAsync(Guid, CancellationToken) instead. Scheduled for removal when EFCore Migration is completed.")]
    async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        Guid? key = await ResolveKeyFromIdAsync(id);
        return key.HasValue && await ExistsAsync(key.Value, cancellationToken);
    }

    /// <summary>
    ///     Gets a language integer ID from its ISO code.
    /// </summary>
    [Obsolete("Use GetKeyByIsoCodeAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    async Task<int?> GetIdByIsoCodeAsync(string? isoCode, bool throwOnNotFound = true)
    {
        Guid? key = await GetKeyByIsoCodeAsync(isoCode, throwOnNotFound);
        return key.HasValue ? await ResolveIdFromKeyAsync(key.Value) : null;
    }

    /// <summary>
    ///     Gets a language ISO code from its integer ID.
    /// </summary>
    [Obsolete("Use GetIsoCodeByKeyAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    async Task<string?> GetIsoCodeByIdAsync(int? id, bool throwOnNotFound = true)
    {
        if (id is null)
        {
            return null;
        }

        Guid? key = await ResolveKeyFromIdAsync(id.Value);
        if (key is null)
        {
            if (throwOnNotFound)
            {
                throw new ArgumentException($"Id {id} does not correspond to an existing language.", nameof(id));
            }

            return null;
        }

        return await GetIsoCodeByKeyAsync(key.Value, throwOnNotFound);
    }

    /// <summary>
    ///     Gets multiple language ISO codes from the provided integer IDs.
    /// </summary>
    [Obsolete("Use GetIsoCodesByKeysAsync instead. Scheduled for removal when EFCore Migration is completed.")]
    async Task<string[]> GetIsoCodesByIdsAsync(ICollection<int> ids, bool throwOnNotFound = true)
    {
        IEnumerable<ILanguage> all = await GetAllAsync(CancellationToken.None);
        var idToKey = all.ToDictionary(x => x.Id, x => x.Key);

        var keys = new List<Guid>(ids.Count);
        foreach (var id in ids)
        {
            if (idToKey.TryGetValue(id, out Guid key))
            {
                keys.Add(key);
            }
            else if (throwOnNotFound)
            {
                throw new ArgumentException($"Id {id} does not correspond to an existing language.", nameof(ids));
            }
            else
            {
                keys.Add(Guid.Empty);
            }
        }

        return await GetIsoCodesByKeysAsync(keys, throwOnNotFound);
    }

    #endregion
}
