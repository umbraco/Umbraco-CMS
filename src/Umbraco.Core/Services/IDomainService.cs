using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IDomainService : IService
{
    bool Exists(string domainName);

    [Obsolete($"Please use {nameof(UpdateDomainsAsync)}. Will be removed in V15")]
    Attempt<OperationResult?> Delete(IDomain domain);

    IDomain? GetByName(string name);

    IDomain? GetById(int id);

    [Obsolete($"Please use {nameof(GetAllAsync)}. Will be removed in V15")]
    IEnumerable<IDomain> GetAll(bool includeWildcards);

    [Obsolete($"Please use {nameof(GetAssignedDomainsAsync)}. Will be removed in V15")]
    IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards);

    [Obsolete($"Please use {nameof(UpdateDomainsAsync)}. Will be removed in V15")]
    Attempt<OperationResult?> Save(IDomain domainEntity);

    [Obsolete($"Please use {nameof(UpdateDomainsAsync)}. Will be removed in V15")]
    Attempt<OperationResult?> Sort(IEnumerable<IDomain> items)
        => Attempt.Fail(new OperationResult(OperationResultType.Failed, new EventMessages())); // TODO Remove default implmentation in a future version

    /// <summary>
    /// Gets all assigned domains for content item.
    /// </summary>
    /// <param name="contentKey">The key of the content item.</param>
    /// <param name="includeWildcards">Whether or not to include wildcard domains.</param>
    Task<IEnumerable<IDomain>> GetAssignedDomainsAsync(Guid contentKey, bool includeWildcards);

    /// <summary>
    /// Gets all assigned domains.
    /// </summary>
    /// <param name="includeWildcards">Whether or not to include wildcard domains.</param>
    Task<IEnumerable<IDomain>> GetAllAsync(bool includeWildcards);

    /// <summary>
    /// Updates the domain assignments for a content item.
    /// </summary>
    /// <param name="contentKey">The key of the content item.</param>
    /// <param name="updateModel">The domain assignments to apply.</param>
    Task<Attempt<DomainUpdateResult, DomainOperationStatus>> UpdateDomainsAsync(Guid contentKey, DomainsUpdateModel updateModel);
}
