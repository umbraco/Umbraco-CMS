using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Services
{
    public interface IAuditService : IService
    {
        void Add(AuditType type, string comment, int userId, int objectId);

        /// <summary>
        /// Returns paged items in the audit trail
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderDirection">
        /// By default this will always be ordered descending (newest first)
        /// </param>
        /// <param name="filter">
        /// Optional filter to be applied
        /// </param>
        /// <returns></returns>
        IEnumerable<IAuditItem> GetPagedItems(int id, long pageIndex, int pageSize, out long totalRecords,
            Direction orderDirection = Direction.Descending, IQuery<IAuditItem> filter = null);
    }
}