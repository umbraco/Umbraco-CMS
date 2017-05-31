using System.Collections.ObjectModel;

namespace Umbraco.Web.Search
{
    public class SearchableTreeCollection : KeyedCollection<string, SearchableApplicationTree>
    {
        protected override string GetKeyForItem(SearchableApplicationTree item)
        {
            return item.TreeAlias;
        }
    }
}