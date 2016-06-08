using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMediaRepository : IRepositoryVersionable<int, IMedia>, IRecycleBinRepository<IMedia>
    {
        IMedia GetMediaByPath(string mediaPath);

        /// <summary>
        /// Gets paged media results
        /// </summary>
        /// <param name="query">Query to excute</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetPagedResultsByQuery(IQuery<IMedia> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IMedia> filter = null);
    }
}