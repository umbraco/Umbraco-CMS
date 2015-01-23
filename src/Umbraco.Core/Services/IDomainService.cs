using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IDomainService : IService
    {
        bool Exists(string domainName);
        void Delete(IDomain domain);
        IDomain GetByName(string name);
        IDomain GetById(int id);
        IEnumerable<IDomain> GetAll(bool includeWildcards);
        IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards);
        void Save(IDomain domainEntity, bool raiseEvents = true);
    }
}