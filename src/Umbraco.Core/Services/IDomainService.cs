using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IDomainService : IService
    {
        bool Exists(string domainName);
        Attempt<OperationStatus> Delete(IDomain domain);
        IDomain GetByName(string name);
        IDomain GetById(int id);
        IEnumerable<IDomain> GetAll(bool includeWildcards);
        IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards);
        Attempt<OperationStatus> Save(IDomain domainEntity);
    }
}