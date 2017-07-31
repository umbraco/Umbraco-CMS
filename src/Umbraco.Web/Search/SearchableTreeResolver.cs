using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// A resolver to return the collection of searchable trees
    /// </summary>
    /// <remarks>
    /// This collection has a request scoped lifetime therefore any instance of ISearchableTree needs to support being a request scoped lifetime
    /// </remarks>
    public class SearchableTreeResolver : LazyManyObjectsResolverBase<SearchableTreeResolver, ISearchableTree>
    {
        private readonly IApplicationTreeService _treeService;

        public SearchableTreeResolver(IServiceProvider serviceProvider, ILogger logger, IApplicationTreeService treeService, Func<IEnumerable<Type>> searchableTrees) 
            : base(serviceProvider, logger, searchableTrees, ObjectLifetimeScope.HttpRequest)
        {
            _treeService = treeService;
        }

        /// <summary>
        /// Returns the a dictionary of tree alias with it's affiliated <see cref="ISearchableTree"/>
        /// </summary>
        public IReadOnlyDictionary<string, SearchableApplicationTree> GetSearchableTrees()
        {
            var appTrees = _treeService.GetAll().ToArray();
            var collection = new SearchableTreeCollection();
            var searchableTrees = Values.ToArray();
            foreach (var searchableTree in searchableTrees)
            {
                var found = appTrees.FirstOrDefault(x => x.Alias == searchableTree.TreeAlias);
                if (found != null)
                {
                    collection.Add(new SearchableApplicationTree(found.ApplicationAlias, found.Alias, searchableTree));
                }
            }
            return collection.AsReadOnlyDictionary();
        }
    }
}
