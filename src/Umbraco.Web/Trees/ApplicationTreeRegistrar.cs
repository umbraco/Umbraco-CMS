using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using umbraco.businesslogic;

namespace Umbraco.Web.Trees
{
    //TODO: Is there any way to get this to execute lazily when needed? 
    // i.e. When the back office loads so that this doesn't execute on startup for a content request.

    /// <summary>
    /// A startup handler for putting the tree config in the config file based on attributes found
    /// </summary>
    public sealed class ApplicationTreeRegistrar : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ScanTrees(applicationContext);
        }

        /// <summary>
        /// Scans for all attributed trees and ensures they exist in the tree xml
        /// </summary>
        private static void ScanTrees(ApplicationContext applicationContext)
        {
            applicationContext.Services.ApplicationTreeService.LoadXml(doc =>
                {
                    var added = new List<string>();

                    // Load all Controller Trees by attribute and add them to the XML config
                    // we also need to make sure that any alias added with the new trees is not also added
                    // with the legacy trees.
                    var types = PluginManager.Current.ResolveAttributedTreeControllers();

                    var items = types
                        .Select(x =>
                                new Tuple<Type, TreeAttribute>(x, x.GetCustomAttributes<TreeAttribute>(false).Single()))
                        .Where(x => applicationContext.Services.ApplicationTreeService.GetByAlias(x.Item2.Alias) == null);

                    foreach (var tuple in items)
                    {
                        var type = tuple.Item1;
                        var attr = tuple.Item2;

                        //Add the new tree that doesn't exist in the config that was found by type finding

                        doc.Root.Add(new XElement("add",
                                                  new XAttribute("initialize", attr.Initialize),
                                                  new XAttribute("sortOrder", attr.SortOrder),
                                                  new XAttribute("alias", attr.Alias),
                                                  new XAttribute("application", attr.ApplicationAlias),
                                                  new XAttribute("title", attr.Title),
                                                  new XAttribute("iconClosed", attr.IconClosed),
                                                  new XAttribute("iconOpen", attr.IconOpen),
                                                  new XAttribute("type", type.GetFullNameWithAssembly())));

                        added.Add(attr.Alias);
                    }


                    // Load all LEGACY Trees by attribute and add them to the XML config
                    var legacyTreeTypes = PluginManager.Current.ResolveAttributedTrees();

                    var legacyItems = legacyTreeTypes
                        .Select(x =>
                                new Tuple<Type, global::umbraco.businesslogic.TreeAttribute>(x, x.GetCustomAttributes<global::umbraco.businesslogic.TreeAttribute>(false).Single()))
                        .Where(x => applicationContext.Services.ApplicationTreeService.GetByAlias(x.Item2.Alias) == null
                                    //make sure the legacy tree isn't added on top of the controller tree!        
                                    && !added.InvariantContains(x.Item2.Alias));

                    foreach (var tuple in legacyItems)
                    {
                        var type = tuple.Item1;
                        var attr = tuple.Item2;

                        //Add the new tree that doesn't exist in the config that was found by type finding
                        doc.Root.Add(new XElement("add",
                                                  new XAttribute("initialize", attr.Initialize),
                                                  new XAttribute("sortOrder", attr.SortOrder),
                                                  new XAttribute("alias", attr.Alias),
                                                  new XAttribute("application", attr.ApplicationAlias),
                                                  new XAttribute("title", attr.Title),
                                                  new XAttribute("iconClosed", attr.IconClosed),
                                                  new XAttribute("iconOpen", attr.IconOpen),
                                                  new XAttribute("type", type.GetFullNameWithAssembly())));
                    }

                }, true);
        }
    }
}
