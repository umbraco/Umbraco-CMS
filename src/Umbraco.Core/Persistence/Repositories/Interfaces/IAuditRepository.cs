using System.Collections;
using System.Collections.Generic;
using Umbraco.Core.Auditing;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IAuditRepository : IRepository<int, IAuditItem>
    {
        IEnumerable<IAuditItem> GetPagedResultsByQuery(
            IQuery<IAuditItem> query,
            long pageIndex, int pageSize, out long totalRecords, 
            Direction orderDirection, IQuery<IAuditItem> customFilter);
    }
}