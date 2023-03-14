using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IDomainCache
{
    /// <summary>
    ///     Gets the system default culture.
    /// </summary>
    string DefaultCulture { get; }

    /// <summary>
    ///     Gets all <see cref="Domain" /> in the current domain cache, including any domains that may be referenced by
    ///     documents that are no longer published.
    /// </summary>
    /// <param name="includeWildcards"></param>
    /// <returns></returns>
    IEnumerable<Domain> GetAll(bool includeWildcards);

    /// <summary>
    ///     Gets all assigned <see cref="Domain" /> for specified document, even if it is not published.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="includeWildcards">A value indicating whether to consider wildcard domains.</param>
    IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false);

    /// <summary>
    ///     Determines whether a document has domains.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="includeWildcards">A value indicating whether to consider wildcard domains.</param>
    bool HasAssigned(int documentId, bool includeWildcards = false);
}
