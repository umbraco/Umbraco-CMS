namespace Umbraco.Cms.Core.Trees;

public class SearchableApplicationTree
{
    public SearchableApplicationTree(string appAlias, string treeAlias, int sortOrder, string formatterService, string formatterMethod, ISearchableTree searchableTree)
    {
        AppAlias = appAlias;
        TreeAlias = treeAlias;
        SortOrder = sortOrder;
        FormatterService = formatterService;
        FormatterMethod = formatterMethod;
        SearchableTree = searchableTree;
    }

    public string AppAlias { get; }

    public string TreeAlias { get; }

    public int SortOrder { get; }

    public string FormatterService { get; }

    public string FormatterMethod { get; }

    public ISearchableTree SearchableTree { get; }
}
