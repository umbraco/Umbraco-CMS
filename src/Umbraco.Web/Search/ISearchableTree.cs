using System;
using System.Collections.Generic;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Search
{
    public interface ISearchableTree
    {
        /// <summary>
        /// The alias of the tree that the <see cref="ISearchableTree"/> belongs to
        /// </summary>
        string TreeAlias { get; }

        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="umbracoHelper"></param>
        /// <param name="query"></param>
        /// <param name="totalFound"></param>
        /// <param name="searchFrom">
        /// A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        IEnumerable<SearchResultItem> Search(UmbracoHelper umbracoHelper, string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null);
    }
}