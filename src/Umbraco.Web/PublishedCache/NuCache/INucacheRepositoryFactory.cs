using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache
{
    /// <summary>
    /// Builds NuCache Repositories
    /// </summary>
    public interface INucacheRepositoryFactory
    {
        /// <summary>
        /// Get an instance of the NuCache document repository
        /// </summary>
        /// <returns></returns>
        INucacheDocumentRepository GetDocumentRepository();
        /// <summary>
        /// Get an instance of the NuCache media repository
        /// </summary>
        /// <returns></returns>
        INucacheMediaRepository GetMediaRepository();
    }
}
