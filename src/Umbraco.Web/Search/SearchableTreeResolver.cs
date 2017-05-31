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
    public class SearchableTreeResolver : LazyManyObjectsResolverBase<SearchableTreeResolver, ISearchableTree>
    {
        private readonly IApplicationTreeService _treeService;

        public SearchableTreeResolver(IServiceProvider serviceProvider, ILogger logger, IApplicationTreeService treeService, Func<IEnumerable<Type>> searchableTrees) 
            : base(serviceProvider, logger, searchableTrees, ObjectLifetimeScope.Application)
        {
            _treeService = treeService;
        }        

        private SearchableTreeCollection _resolved;
        private static readonly object Locker = new object();

        /// <summary>
        /// Returns the a dictionary of tree alias with it's affiliated <see cref="ISearchableTree"/>
        /// </summary>
        public SearchableTreeCollection SearchableTrees
        {
            get
            {
                if (_resolved != null) return _resolved;

                lock (Locker)
                {
                    var appTrees = _treeService.GetAll().ToArray();
                    _resolved = new SearchableTreeCollection();
                    var searchableTrees = Values.ToArray();
                    foreach (var instanceType in InstanceTypes)
                    {
                        if (TypeHelper.IsTypeAssignableFrom<ISearchableTree>(instanceType))
                        {
                            var found = appTrees.FirstOrDefault(x => x.GetRuntimeType() == instanceType);
                            if (found != null)
                            {
                                _resolved.Add(new SearchableApplicationTree(found.ApplicationAlias, found.Alias, searchableTrees.First(x => x.GetType() == instanceType)));
                            }
                        }
                    }
                    return _resolved;
                }
            }
        }

    }
}
