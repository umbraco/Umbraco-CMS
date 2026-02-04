using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IDomain" /> entities.
/// </summary>
public interface IDomainRepository : IReadWriteQueryRepository<int, IDomain>
{
    /// <summary>
    ///     Gets a domain by its name.
    /// </summary>
    /// <param name="domainName">The name of the domain.</param>
    /// <returns>The domain if found; otherwise, <c>null</c>.</returns>
    IDomain? GetByName(string domainName);

    /// <summary>
    ///     Checks whether a domain with the specified name exists.
    /// </summary>
    /// <param name="domainName">The name of the domain.</param>
    /// <returns><c>true</c> if a domain with the name exists; otherwise, <c>false</c>.</returns>
    bool Exists(string domainName);

    /// <summary>
    ///     Gets all domains.
    /// </summary>
    /// <param name="includeWildcards">Whether to include wildcard domains.</param>
    /// <returns>A collection of domains.</returns>
    IEnumerable<IDomain> GetAll(bool includeWildcards);

    /// <summary>
    ///     Gets all domains assigned to a content item.
    /// </summary>
    /// <param name="contentId">The identifier of the content item.</param>
    /// <param name="includeWildcards">Whether to include wildcard domains.</param>
    /// <returns>A collection of domains assigned to the content.</returns>
    IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards);
}
