using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Umbraco.Web.Search
{
    internal class SearchableTreeCollection : KeyedCollection<string, SearchableApplicationTree>
    {
        protected override string GetKeyForItem(SearchableApplicationTree item)
        {
            return item.TreeAlias;
        }

        public IReadOnlyDictionary<string, SearchableApplicationTree> AsReadOnlyDictionary()
        {
            return new ReadOnlyDictionary<string, SearchableApplicationTree>(Dictionary);
        }
    }
}