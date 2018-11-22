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

        public string AppAlias { get; private set; }
        public string TreeAlias { get; private set; }
        public ISearchableTree SearchableTree { get; private set; }
        
    }
}