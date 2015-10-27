using System.Collections;
using Umbraco.Core.Auditing;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IAuditRepository : IRepository<int, AuditItem>
    {
        
    }
}