namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Defines operations for handling document URL aliases (umbracoUrlAlias property).
/// </summary>
public interface IDocumentUrlAliasService
{
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
}
