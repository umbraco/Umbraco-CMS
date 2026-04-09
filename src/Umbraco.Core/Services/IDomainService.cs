using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for managing domains (hostnames) assigned to content items.
/// </summary>
/// <remarks>
///     Domains are used to configure multi-site setups where different hostnames
///     route to different content nodes in the content tree.
/// </remarks>
public interface IDomainService : IService
{
    /// <summary>
    ///     Checks if a domain with the specified name exists.
    /// </summary>
    /// <param name="domainName">The domain name to check.</param>
    /// <returns><c>true</c> if the domain exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(string domainName);

    /// <summary>
    ///     Gets a domain by its name.
    /// </summary>
    /// <param name="name">The domain name.</param>
    /// <returns>The domain, or <c>null</c> if not found.</returns>
    Task<IDomain?> GetByNameAsync(string name);

    /// <summary>
    ///     Gets a domain by its identifier.
    /// </summary>
    /// <param name="id">The domain identifier.</param>
    /// <returns>The domain, or <c>null</c> if not found.</returns>
    [Obsolete("Use key-based overloads instead. Scheduled for removal when EFCore migration is completed.")]
    Task<IDomain?> GetByIdAsync(int id);

    /// <summary>
    ///     Gets a domain by its key.
    /// </summary>
    /// <param name="key">The domain key.</param>
    /// <returns>The domain, or <c>null</c> if not found.</returns>
    Task<IDomain?> GetByKeyAsync(Guid key);

    /// <summary>
    ///     Gets all assigned domains for a content item by its key.
    /// </summary>
    /// <param name="contentKey">The unique identifier of the content item.</param>
    /// <param name="includeWildcards">Whether to include wildcard domains.</param>
    /// <returns>A collection of domains assigned to the content item.</returns>
    Task<IEnumerable<IDomain>> GetAssignedDomainsAsync(Guid contentKey, bool includeWildcards);

    /// <summary>
    ///     Gets all assigned domains for a content item by its integer identifier.
    /// </summary>
    /// <param name="contentId">The integer identifier of the content item.</param>
    /// <param name="includeWildcards">Whether to include wildcard domains.</param>
    /// <returns>A collection of domains assigned to the content item.</returns>
    [Obsolete("Use the Guid key overload instead. Scheduled for removal when EFCore migration is completed.")]
    Task<IEnumerable<IDomain>> GetAssignedDomainsAsync(int contentId, bool includeWildcards);

    /// <summary>
    ///     Gets all assigned domains.
    /// </summary>
    /// <param name="includeWildcards">Whether to include wildcard domains.</param>
    /// <returns>A collection of all domains.</returns>
    Task<IEnumerable<IDomain>> GetAllAsync(bool includeWildcards);

    /// <summary>
    ///     Updates the domain assignments for a content item.
    /// </summary>
    /// <param name="contentKey">The unique identifier of the content item.</param>
    /// <param name="updateModel">The domain assignments to apply.</param>
    /// <returns>An attempt containing the update result or an error status.</returns>
    Task<Attempt<DomainUpdateResult, DomainOperationStatus>> UpdateDomainsAsync(Guid contentKey, DomainsUpdateModel updateModel);
}
