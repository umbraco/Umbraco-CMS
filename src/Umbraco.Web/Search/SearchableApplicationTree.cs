using Umbraco.Web.Trees;

namespace Umbraco.Web.Search
{
    public class SearchableApplicationTree
    {
        public SearchableApplicationTree(string appAlias, string treeAlias, ISearchableTree searchableTree)
        {
            AppAlias = appAlias;
            TreeAlias = treeAlias;
            SearchableTree = searchableTree;
        }

        public string AppAlias { get; }
        public string TreeAlias { get; }
        public ISearchableTree SearchableTree { get; }
    }
}
