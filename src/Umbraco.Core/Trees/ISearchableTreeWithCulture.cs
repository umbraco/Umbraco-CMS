using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Trees
{
    [Obsolete("This interface will be merged into ISearchableTree in Umbraco 12")]
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
