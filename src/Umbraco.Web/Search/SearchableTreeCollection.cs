﻿using System;
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

        public SearchableTreeCollection(IEnumerable<ISearchableTree> items, ITreeService treeService)
            : base(items)
        {
            _dictionary = CreateDictionary(treeService);
        }

        private Dictionary<string, SearchableApplicationTree> CreateDictionary(ITreeService treeService)
        {
            var appTrees = treeService.GetAll()
                .OrderBy(x => x.SortOrder)
                .ToArray();
            var dictionary = new Dictionary<string, SearchableApplicationTree>(StringComparer.OrdinalIgnoreCase);
            var searchableTrees = this.ToArray();
            foreach (var appTree in appTrees)
            {
                var found = searchableTrees.FirstOrDefault(x => x.TreeAlias.InvariantEquals(appTree.TreeAlias));
                if (found != null)
                {
                    var searchableTreeAttribute = found.GetType().GetCustomAttribute<SearchableTreeAttribute>(false);
                    dictionary[found.TreeAlias] = new SearchableApplicationTree(
                        appTree.SectionAlias,
                        appTree.TreeAlias,
                        searchableTreeAttribute?.SortOrder ?? SearchableTreeAttribute.DefaultSortOrder,
                        searchableTreeAttribute?.ServiceName ?? string.Empty,
                        searchableTreeAttribute?.MethodName ?? string.Empty,
                        found
                    );
                }
            }
            return dictionary;
        }

        public IReadOnlyDictionary<string, SearchableApplicationTree> SearchableApplicationTrees => _dictionary;

        public SearchableApplicationTree this[string key] => _dictionary[key];
    }
}
