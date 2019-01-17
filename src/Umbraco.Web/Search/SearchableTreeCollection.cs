using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Web.Services;
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
            var appTrees = treeService.GetAll()
                .OrderBy(x => x.SortOrder)
                .ToArray();
            var dictionary = new Dictionary<string, SearchableApplicationTree>(StringComparer.OrdinalIgnoreCase);
            var searchableTrees = this.ToArray();
            foreach (var appTree in appTrees)
            {
                var found = searchableTrees.FirstOrDefault(x => x.TreeAlias.InvariantEquals(appTree.Alias));
                if (found != null)
                {
                    dictionary[found.TreeAlias] = new SearchableApplicationTree(appTree.ApplicationAlias, appTree.Alias, found);
                }
            }
            return dictionary;
        }

        public IReadOnlyDictionary<string, SearchableApplicationTree> SearchableApplicationTrees => _dictionary;

        public SearchableApplicationTree this[string key] => _dictionary[key];
    }
}
