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
        
        /// <summary>
        /// Used to add/update published xml for the media item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        void AddOrUpdateContentXml(IMedia content, Func<IMedia, XElement> xml);

        /// <summary>
        /// Used to add/update preview xml for the content item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        void AddOrUpdatePreviewXml(IMedia content, Func<IMedia, XElement> xml);

        /// <summary>
        /// Gets paged media results
        /// </summary>
        /// <param name="query">Query to excute</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetPagedResultsByQuery(IQuery<IMedia> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, string filter = "");
    }
}