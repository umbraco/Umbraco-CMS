using System.Collections.Generic;

namespace Umbraco.Web.Trees
{
    public interface ISearchableTree
    {
        IEnumerable<SearchResultItem> Search(string searchText);
    }
}