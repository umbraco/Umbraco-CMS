using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache
{
    /// <summary>
    /// Nucache repository for documents.
    /// </summary>
    /// <remarks>Ensure the repository is responisble for queries. The underlying storage may support more efficent queries</remarks>
    public interface INucacheDocumentRepository : INucacheRepositoryBase<int, ContentNodeKit>
    {
        /// <summary>
        /// Get All by level, parentid, sortorder
        /// </summary>
        /// <returns></returns>
        ICollection<ContentNodeKit> GetAllSorted();
    }
}
