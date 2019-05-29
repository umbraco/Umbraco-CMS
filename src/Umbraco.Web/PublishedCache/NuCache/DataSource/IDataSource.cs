using System.Collections.Generic;
using Umbraco.Core.Scoping;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Defines a data source for NuCache.
    /// </summary>
    internal interface IDataSource
    {
        ContentNodeKit GetContentSource(IScope scope, int id);
        IEnumerable<ContentNodeKit> GetAllContentSources(IScope scope); // must order by level
        IEnumerable<ContentNodeKit> GetBranchContentSources(IScope scope, int id); // must order by level, sortOrder
        IEnumerable<ContentNodeKit> GetTypeContentSources(IScope scope, IEnumerable<int> ids);

        ContentNodeKit GetMediaSource(IScope scope, int id);
        IEnumerable<ContentNodeKit> GetAllMediaSources(IScope scope); // must order by level
        IEnumerable<ContentNodeKit> GetBranchMediaSources(IScope scope, int id); // must order by level, sortOrder
        IEnumerable<ContentNodeKit> GetTypeMediaSources(IScope scope, IEnumerable<int> ids);
    }
}
