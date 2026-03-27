using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IDomain" /> entities.
/// </summary>
public interface IDomainRepository : IAsyncReadWriteRepository<Guid, IDomain>
{
    /// <summary>
    ///     Gets a domain by its name.
    /// </summary>
    /// <param name="domainName">The name of the domain.</param>
    /// <returns>The domain if found; otherwise, <c>null</c>.</returns>
    Task<IDomain?> GetByNameAsync(string domainName);

    /// <summary>
    ///     Checks whether a domain with the specified name exists.
    /// </summary>
    /// <param name="domainName">The name of the domain.</param>
    /// <returns><c>true</c> if a domain with the name exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(string domainName);

    /// <summary>
    ///     Gets all domains.
    /// </summary>
    /// <param name="includeWildcards">Whether to include wildcard domains.</param>
    /// <returns>A collection of domains.</returns>
    Task<IEnumerable<IDomain>> GetAllAsync(bool includeWildcards);

    /// <summary>
    ///     Gets all domains assigned to a content item.
    /// </summary>
    /// <param name="contentKey">The key of the content item.</param>
    /// <param name="includeWildcards">Whether to include wildcard domains.</param>
    /// <returns>A collection of domains assigned to the content.</returns>
    Task<IEnumerable<IDomain>> GetAssignedDomainsAsync(Guid contentKey, bool includeWildcards);
}
