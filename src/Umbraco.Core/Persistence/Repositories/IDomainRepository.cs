using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IDomainRepository : IAsyncReadWriteQueryRepository<int, IDomain>
    {
        IDomain? GetByName(string domainName);
        bool Exists(string domainName);
        IEnumerable<IDomain> GetAll(bool includeWildcards);
        IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards);
    }
}
