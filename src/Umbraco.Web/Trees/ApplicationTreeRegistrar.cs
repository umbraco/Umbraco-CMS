using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
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
            var added = new List<string>();

            // Load all Controller Trees by attribute and add them to the XML config
            // we also need to make sure that any alias added with the new trees is not also added
            // with the legacy trees.
            var types = PluginManager.Current.ResolveAttributedTreeControllers();

            //get all non-legacy application tree's
            var items = types
                .Select(x =>
                        new Tuple<Type, TreeAttribute>(x, x.GetCustomAttributes<TreeAttribute>(false).Single()))
                .Where(x => applicationContext.Services.ApplicationTreeService.GetByAlias(x.Item2.Alias) == null)
                .Select(x => new ApplicationTree(x.Item2.Initialize, x.Item2.SortOrder, x.Item2.ApplicationAlias, x.Item2.Alias, x.Item2.Title, x.Item2.IconClosed, x.Item2.IconOpen, x.Item1.GetFullNameWithAssembly()))
                .ToArray();
                
            added.AddRange(items.Select(x => x.Alias));

            //find the legacy trees
            var legacyTreeTypes = PluginManager.Current.ResolveAttributedTrees();

            var legacyItems = legacyTreeTypes
                .Select(x =>
                        new Tuple<Type, global::umbraco.businesslogic.TreeAttribute>(
                            x, 
                            x.GetCustomAttributes<global::umbraco.businesslogic.TreeAttribute>(false).SingleOrDefault()))
                .Where(x => x.Item2 != null)
                .Where(x => applicationContext.Services.ApplicationTreeService.GetByAlias(x.Item2.Alias) == null
                    //make sure the legacy tree isn't added on top of the controller tree!        
                            && added.InvariantContains(x.Item2.Alias) == false)
                .Select(x => new ApplicationTree(x.Item2.Initialize, x.Item2.SortOrder, x.Item2.ApplicationAlias, x.Item2.Alias, x.Item2.Title, x.Item2.IconClosed, x.Item2.IconOpen, x.Item1.GetFullNameWithAssembly()));

            applicationContext.Services.ApplicationTreeService.Intitialize(items.Concat(legacyItems));

        }
    }
}
