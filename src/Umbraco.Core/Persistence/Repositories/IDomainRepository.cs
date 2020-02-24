﻿using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDomainRepository : IReadWriteQueryRepository<int, IDomain>
    {
        IDomain GetByName(string domainName);
        bool Exists(string domainName);
        IEnumerable<IDomain> GetAll(bool includeWildcards);
        IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards);
    }
}
