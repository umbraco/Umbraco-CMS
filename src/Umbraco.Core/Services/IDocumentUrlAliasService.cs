using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Defines operations for handling document URL aliases (umbracoUrlAlias property).
/// </summary>
public interface IDocumentUrlAliasService
{
    /// <summary>
    /// Gets a value indicating whether the service has been initialized and may persist aliases.
    /// </summary>
    /// <remarks>
    /// False while the application is upgrading, when the alias table may not exist yet.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    bool IsInitialized => true;

    /// <summary>
    /// Initializes the service and ensures the alias cache is populated from the database.
    /// </summary>
    /// <param name="forceEmpty">Forces an early return when we know there are no aliases (i.e. on install).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task InitAsync(bool forceEmpty, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all document keys that match a given URL alias.
    /// </summary>
    /// <param name="alias">The URL alias (normalized: lowercase, no leading slash).</param>
    /// <param name="culture">The culture code (null for invariant).</param>
    /// <returns>All document keys that have the specified alias, or empty if none found.</returns>
    Task<IEnumerable<Guid>> GetDocumentKeysByAliasAsync(string alias, string? culture);

    /// <summary>
    /// Gets all URL aliases for a given document.
    /// </summary>
    /// <param name="documentKey">The document key.</param>
    /// <param name="culture">The culture code (null for default language).</param>
    /// <returns>All aliases for the document in the specified culture, or empty if none found.</returns>
    Task<IEnumerable<string>> GetAliasesAsync(Guid documentKey, string? culture);

    /// <summary>
    /// Creates or updates the aliases for a single document.
    /// </summary>
    /// <param name="documentKey">The document key.</param>
    Task CreateOrUpdateAliasesAsync(Guid documentKey);

    /// <summary>
    /// Creates or updates the aliases for a document and its descendants.
    /// </summary>
    /// <param name="documentKey">The document key.</param>
    Task CreateOrUpdateAliasesWithDescendantsAsync(Guid documentKey);

    /// <summary>
    /// Persists the aliases of a document from the document itself, taking the locks that order the write against
    /// saves and against <see cref="RebuildAllAliasesAsync"/>.
    /// </summary>
    /// <param name="document">The document whose aliases to persist.</param>
    /// <returns>A task that completes when the aliases are persisted.</returns>
    // TODO (V19): Remove the default implementation.
    Task PersistAliasesAsync(IContent document)
        => PersistAliasesAsync(document, contentTreeWriteLockHeld: false);

    /// <summary>
    /// Persists the aliases of a document from the document itself.
    /// </summary>
    /// <param name="document">The document whose aliases to persist.</param>
    /// <param name="contentTreeWriteLockHeld">
    /// True only when the caller holds the content tree write lock, as the transaction that persists
    /// <paramref name="document"/> does; the write then needs no lock of its own. Any other caller passes false
    /// and the implementation takes the content tree read lock and the alias write lock itself.
    /// </param>
    /// <remarks>
    /// The CMS calls this from the document repository's refresh notification, inside the transaction that persists
    /// <paramref name="document"/>. Only a change to the document's published state or trashed state can change its
    /// aliases, so any other save is a no-op. Implementations must work from <paramref name="document"/> itself:
    /// inside the content transaction the repository caches still hold the document as it was before the save, so
    /// loading it by key would persist the previous aliases. The default implementation only keeps implementations
    /// written before this member compiling; it is not a correct implementation of the contract.
    /// </remarks>
    /// <returns>A task that completes when the aliases are persisted.</returns>
    // TODO (V19): Remove the default implementation.
    Task PersistAliasesAsync(IContent document, bool contentTreeWriteLockHeld)
        => CreateOrUpdateAliasesAsync(document.Key);

    /// <summary>
    /// Deletes all aliases from the cache for a collection of document keys.
    /// </summary>
    /// <param name="documentKeys">The collection of document keys.</param>
    Task DeleteAliasesFromCacheAsync(IEnumerable<Guid> documentKeys);

    /// <summary>
    /// Rebuilds all URL aliases from the database.
    /// </summary>
    /// <remarks>
    /// This method clears the existing alias cache and database records,
    /// then rebuilds from the umbracoUrlAlias property values on all documents.
    /// </remarks>
    Task RebuildAllAliasesAsync();

    /// <summary>
    /// Checks whether any aliases are cached.
    /// </summary>
    /// <returns><c>true</c> if there are any aliases in the cache; otherwise, <c>false</c>.</returns>
    bool HasAny();

    /// <summary>
    /// Updates the in-memory alias cache for a single document without writing to the database.
    /// </summary>
    /// <param name="documentKey">The document key.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAliasCacheAsync(Guid documentKey);

    /// <summary>
    /// Updates the in-memory alias cache for a document and its descendants without writing to the database.
    /// </summary>
    /// <param name="documentKey">The document key.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAliasCacheWithDescendantsAsync(Guid documentKey);
}
