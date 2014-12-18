using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.BusinessLogic
{
    public class ApplicationTreeRegistrar : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Call initialize on the tree service with the lazy enumerable class below, when the tree service needs to resolve,
            // it will lazily do the scanning and comparing so it's not actually done on app start.
            ApplicationTree.Intitialize(new LazyEnumerableTrees());
        }

        //public ApplicationTreeRegistrar()
        //{
        //    //don't do anything if the application or database is not configured!
        //    if (ApplicationContext.Current == null 
        //        || !ApplicationContext.Current.IsConfigured 
        //        || !ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured)
        //        return;

        //    // Load all Trees by attribute and add them to the XML config
        //    var types = PluginManager.Current.ResolveAttributedTrees();

        //    var items = types
        //        .Select(x =>
        //                new Tuple<Type, TreeAttribute>(x, x.GetCustomAttributes<TreeAttribute>(false).Single()))
        //        .Where(x => ApplicationTree.getByAlias(x.Item2.Alias) == null);

        //    var allAliases = ApplicationTree.getAll().Select(x => x.Alias).Concat(items.Select(x => x.Item2.Alias));
        //    var inString = "'" + string.Join("','", allAliases) + "'";

        //    ApplicationTree.LoadXml(doc =>
        //    {
        //        foreach (var tuple in items)
        //        {
        //            var type = tuple.Item1;
        //            var attr = tuple.Item2;

        //            //Add the new tree that doesn't exist in the config that was found by type finding

        //            doc.Root.Add(new XElement("add",
        //                                      new XAttribute("silent", attr.Silent),
        //                                      new XAttribute("initialize", attr.Initialize),
        //                                      new XAttribute("sortOrder", attr.SortOrder),
        //                                      new XAttribute("alias", attr.Alias),
        //                                      new XAttribute("application", attr.ApplicationAlias),
        //                                      new XAttribute("title", attr.Title),
        //                                      new XAttribute("iconClosed", attr.IconClosed),
        //                                      new XAttribute("iconOpen", attr.IconOpen),
        //                                      // don't add the assembly, we don't need this:
        //                                      //	http://issues.umbraco.org/issue/U4-1360
        //                                      //new XAttribute("assembly", assemblyName),
        //                                      //new XAttribute("type", typeName),
        //                                      // instead, store the assembly type name
        //                                      new XAttribute("type", type.GetFullNameWithAssembly()),
        //                                      new XAttribute("action", attr.Action)));
        //        }

        //        //add any trees that were found in the database that don't exist in the config

        //        var db = ApplicationContext.Current.DatabaseContext.Database;
        //        var exist = db.TableExist("umbracoAppTree");
        //        if (exist)
        //        {
        //            var appTrees = db.Fetch<AppTreeDto>("WHERE treeAlias NOT IN (" + inString + ")");
        //            foreach (var appTree in appTrees)
        //            {
        //                var action = appTree.Action;

        //                doc.Root.Add(new XElement("add",
        //                                          new XAttribute("silent", appTree.Silent),
        //                                          new XAttribute("initialize", appTree.Initialize),
        //                                          new XAttribute("sortOrder", appTree.SortOrder),
        //                                          new XAttribute("alias", appTree.Alias),
        //                                          new XAttribute("application", appTree.AppAlias),
        //                                          new XAttribute("title", appTree.Title),
        //                                          new XAttribute("iconClosed", appTree.IconClosed),
        //                                          new XAttribute("iconOpen", appTree.IconOpen),
        //                                          new XAttribute("assembly", appTree.HandlerAssembly),
        //                                          new XAttribute("type", appTree.HandlerType),
        //                                          new XAttribute("action", string.IsNullOrEmpty(action) ? "" : action)));
        //            }
        //        }

        //    }, true);
        //}

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
                    var types = PluginManager.Current.ResolveAttributedTrees();
                    //convert them to ApplicationTree instances
                    var items = types
                        .Select(x =>
                            new Tuple<Type, TreeAttribute>(x, x.GetCustomAttributes<TreeAttribute>(false).Single()))
                        .Select(x => new ApplicationTree(
                            x.Item2.Silent, x.Item2.Initialize, (byte) x.Item2.SortOrder, x.Item2.ApplicationAlias, x.Item2.Alias, x.Item2.Title, x.Item2.IconClosed, x.Item2.IconOpen,
                            "",
                            x.Item1.GetFullNameWithAssembly(),
                            x.Item2.Action))
                        .ToArray();

                    added.AddRange(items.Select(x => x.Alias));

                    //find the legacy trees
                    var legacyTreeTypes = PluginManager.Current.ResolveAttributedTrees();
                    //convert them to ApplicationTree instances
                    var legacyItems = legacyTreeTypes
                        .Select(x =>
                            new Tuple<Type, global::umbraco.businesslogic.TreeAttribute>(
                                x,
                                x.GetCustomAttributes<global::umbraco.businesslogic.TreeAttribute>(false).SingleOrDefault()))
                        .Where(x => x.Item2 != null)
                        //make sure the legacy tree isn't added on top of the controller tree!        
                        .Where(x => added.InvariantContains(x.Item2.Alias) == false)
                        .Select(x => new ApplicationTree(x.Item2.Silent, x.Item2.Initialize, (byte) x.Item2.SortOrder, x.Item2.ApplicationAlias, x.Item2.Alias, x.Item2.Title, x.Item2.IconClosed, x.Item2.IconOpen,
                            "",
                            x.Item1.GetFullNameWithAssembly(),
                            x.Item2.Action));

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