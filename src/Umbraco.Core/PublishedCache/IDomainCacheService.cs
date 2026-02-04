using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Defines operations for the domain cache service.
/// </summary>
/// <remarks>
/// This service provides access to domain information with caching support,
/// including operations for retrieving domains and handling cache refresh.
/// </remarks>
public interface IDomainCacheService
{
    /// <summary>
    ///     Gets all <see cref="Domain" /> in the current domain cache, including any domains that may be referenced by
    ///     documents that are no longer published.
    /// </summary>
    /// <param name="includeWildcards">A value indicating whether to include wildcard domains.</param>
    /// <returns>A collection of all domains in the cache.</returns>
    IEnumerable<Domain> GetAll(bool includeWildcards);

    /// <summary>
    ///     Gets all assigned <see cref="Domain" /> for specified document, even if it is not published.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="includeWildcards">A value indicating whether to consider wildcard domains.</param>
    /// <returns>A collection of domains assigned to the specified document.</returns>
    IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false);

    /// <summary>
    ///     Determines whether a document has domains.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="includeWildcards">A value indicating whether to consider wildcard domains.</param>
    /// <returns><c>true</c> if the document has assigned domains; otherwise, <c>false</c>.</returns>
    bool HasAssigned(int documentId, bool includeWildcards = false);

    /// <summary>
    /// Refreshes the domain cache based on the provided payloads.
    /// </summary>
    /// <param name="payloads">The cache refresh payloads containing domain change information.</param>
    void Refresh(DomainCacheRefresher.JsonPayload[] payloads);
}
