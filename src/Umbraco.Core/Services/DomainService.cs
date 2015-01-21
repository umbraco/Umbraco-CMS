using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class DomainService : RepositoryService, IDomainService
    {
        public DomainService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger)
            : base(provider, repositoryFactory, logger)
        {
        }

        public IEnumerable<IDomain> GetAll(bool includeWildcards)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDomain> GetDomainsForContent(int contentId, bool includeWildcards)
        {
            throw new NotImplementedException();
        }
    }
}