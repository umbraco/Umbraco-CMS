using System.Collections.Generic;
using Umbraco.Core.Scoping;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Defines a data source for NuCache.
    /// </summary>
    internal interface IDataSource
    {
        //TODO: For these required sort orders, would sorting on Path 'just work'?

        ContentNodeKit GetContentSource(IScope scope, int id);

        /// <summary>
        /// Returns all content ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetAllContentSources(IScope scope);

        /// <summary>
        /// Returns branch for content ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetBranchContentSources(IScope scope, int id);

        /// <summary>
        /// Returns content by Ids ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetTypeContentSources(IScope scope, IEnumerable<int> ids);

        ContentNodeKit GetMediaSource(IScope scope, int id);

        /// <summary>
        /// Returns all media ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetAllMediaSources(IScope scope);

        /// <summary>
        /// Returns branch for media ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetBranchMediaSources(IScope scope, int id); // must order by level, sortOrder

        /// <summary>
        /// Returns media by Ids ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetTypeMediaSources(IScope scope, IEnumerable<int> ids);
    }
}
