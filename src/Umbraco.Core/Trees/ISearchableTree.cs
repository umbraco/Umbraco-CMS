using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Trees
{
    public interface ISearchableTree : IDiscoverable
    {
        /// <summary>
        /// The alias of the tree that the <see cref="ISearchableTree"/> belongs to
        /// </summary>
        string TreeAlias { get; }

        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="totalFound"></param>
        /// <param name="searchFrom">
        ///     A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <returns></returns>
        Task<EntitySearchResults> SearchAsync(string query, int pageSize, long pageIndex, string? searchFrom = null);
    }

    // TODO: eventually this interface should be merged into ISearchableTree by adding the culture parameter to
    //       ISearchableTree.SearchAsync. however, this is a breaking change, so we have to wait until that is allowed.
    [Obsolete("This interface will be merged into ISearchableTree")]
    public interface ISearchableTreeWithCulture : ISearchableTree
    {
        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="searchFrom">
        ///     A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <param name="culture"></param>
        /// <returns></returns>
        Task<EntitySearchResults> SearchAsync(string query, int pageSize, long pageIndex, string? searchFrom = null, string? culture = null);
    }
}
