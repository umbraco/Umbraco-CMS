using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IDomainService : IService
{
    bool Exists(string domainName);

    Attempt<OperationResult?> Delete(IDomain domain);

    IDomain? GetByName(string name);

    IDomain? GetById(int id);

    IEnumerable<IDomain> GetAll(bool includeWildcards);

    IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards);

    Attempt<OperationResult?> Save(IDomain domainEntity);

    Attempt<OperationResult?> Sort(IEnumerable<IDomain> items)
        => Attempt.Fail(new OperationResult(OperationResultType.Failed, new EventMessages())); // TODO Remove default implmentation in a future version
}
