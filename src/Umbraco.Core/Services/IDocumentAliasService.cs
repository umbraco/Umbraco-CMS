namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Defines operations for handling document URL aliases (umbracoUrlAlias property).
/// </summary>
public interface IDocumentAliasService
{
    /// <summary>
    /// Initializes the service and ensures the alias cache is populated from the database.
    /// </summary>
    /// <param name="forceEmpty">Forces an early return when we know there are no aliases (i.e. on install).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task InitAsync(bool forceEmpty, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the document key for a given URL alias.
    /// </summary>
    /// <param name="alias">The URL alias (normalized: lowercase, no leading slash).</param>
    /// <param name="culture">The culture code (null for invariant).</param>
    /// <param name="domainRootKey">The domain root key for scoping (null for all domains).</param>
    /// <returns>The document key, or null if not found.</returns>
    Guid? GetDocumentKeyByAlias(string alias, string? culture, Guid? domainRootKey);

    /// <summary>
    /// Gets all aliases for a document.
    /// </summary>
    /// <param name="documentKey">The document key.</param>
    /// <param name="culture">The culture code (null for invariant).</param>
    /// <returns>The aliases for the document.</returns>
    IEnumerable<string> GetAliases(Guid documentKey, string? culture);

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
    /// Deletes all aliases from the cache for a collection of document keys.
    /// </summary>
    /// <param name="documentKeys">The collection of document keys.</param>
    Task DeleteAliasesFromCacheAsync(IEnumerable<Guid> documentKeys);

    /// <summary>
    /// Gets a value indicating whether any aliases have been cached.
    /// </summary>
    bool HasAny();
}
