using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using umbraco.businesslogic;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// A startup handler for putting the tree config in the config file based on attributes found
    /// </summary>
    /// <remarks>
    /// TODO: This is really not a very ideal process but the code is found here because tree plugins are in the Web project or the legacy business logic project.
    /// Moving forward we can put the base tree plugin classes in the core and then this can all just be taken care of normally within the service.
    /// </remarks>
    public sealed class ApplicationTreeRegistrar : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Call initialize on the tree service with the lazy enumerable class below, when the tree service needs to resolve,
            // it will lazily do the scanning and comparing so it's not actually done on app start.
            applicationContext.Services.ApplicationTreeService.Intitialize(new LazyEnumerableTrees());
        }

        /// <summary>
        /// This class is here so that we can provide lazy access to tree scanning for when it is needed
        /// </summary>
        private class LazyEnumerableTrees : IEnumerable<ApplicationTree>
        {
            public LazyEnumerableTrees()
            {
                _lazyTrees = new Lazy<IEnumerable<ApplicationTree>>(() =>
                {
                    var added = new List<string>();

                    // Load all Controller Trees by attribute
                    var types = PluginManager.Current.ResolveAttributedTreeControllers();
                    //convert them to ApplicationTree instances
                    var items = types
                        .Select(x =>
                                new Tuple<Type, TreeAttribute>(x, x.GetCustomAttributes<TreeAttribute>(false).Single()))
                        .Select(x => new ApplicationTree(x.Item2.Initialize, x.Item2.SortOrder, x.Item2.ApplicationAlias, x.Item2.Alias, x.Item2.Title, x.Item2.IconClosed, x.Item2.IconOpen, x.Item1.GetFullNameWithAssembly()))
                        .ToArray();

                    added.AddRange(items.Select(x => x.Alias));

                    //find the legacy trees
                    var legacyTreeTypes = PluginManager.Current.ResolveAttributedTrees();
                    //convert them to ApplicationTree instances
                    var legacyItems = legacyTreeTypes
                        .Select(x =>
                            new Tuple<Type, global::umbraco.businesslogic.TreeAttribute, ObsoleteAttribute>(
                                x,
                                x.GetCustomAttributes<global::umbraco.businesslogic.TreeAttribute>(false).SingleOrDefault(),
                                x.GetCustomAttributes<ObsoleteAttribute>(false).SingleOrDefault()))
                        //ensure that the legacy tree attribute exists
                        .Where(x => x.Item2 != null)
                        //ensure that it's not obsoleted, any obsoleted tree will not be auto added to the config
                        .Where(x => x.Item3 == null)
                        //make sure the legacy tree isn't added on top of the controller tree!        
                        .Where(x => added.InvariantContains(x.Item2.Alias) == false)
                        .Select(x => new ApplicationTree(x.Item2.Initialize, x.Item2.SortOrder, x.Item2.ApplicationAlias, x.Item2.Alias, x.Item2.Title, x.Item2.IconClosed, x.Item2.IconOpen, x.Item1.GetFullNameWithAssembly()));

                    return items.Concat(legacyItems).ToArray();
                });
            }

            private readonly Lazy<IEnumerable<ApplicationTree>> _lazyTrees;

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<ApplicationTree> GetEnumerator()
            {
                return _lazyTrees.Value.GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
