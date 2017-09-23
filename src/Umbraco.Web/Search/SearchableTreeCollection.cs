using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Search
{
    public class SearchableTreeCollection : BuilderCollectionBase<ISearchableTree>
    {
        private readonly Dictionary<string, SearchableApplicationTree> _dictionary;

        public SearchableTreeCollection(IEnumerable<ISearchableTree> items, IApplicationTreeService treeService)
            : base(items)
        {
            _dictionary = CreateDictionary(treeService);
        }

        private Dictionary<string, SearchableApplicationTree> CreateDictionary(IApplicationTreeService treeService)
        {
            var appTrees = treeService.GetAll().ToArray();
            var dictionary = new Dictionary<string, SearchableApplicationTree>();
            var searchableTrees = this.ToArray();
            foreach (var searchableTree in searchableTrees)
            {
                var found = appTrees.FirstOrDefault(x => x.Alias == searchableTree.TreeAlias);
                if (found != null)
                {
                    dictionary[searchableTree.TreeAlias] = new SearchableApplicationTree(found.ApplicationAlias, found.Alias, searchableTree);
                }
            }
            return dictionary;
        }

        // fixme - oh why?!
        public IReadOnlyDictionary<string, SearchableApplicationTree> AsReadOnlyDictionary()
        {
            return new ReadOnlyDictionary<string, SearchableApplicationTree>(_dictionary);
        }

        public SearchableApplicationTree this[string key] => _dictionary[key];
    }
}
