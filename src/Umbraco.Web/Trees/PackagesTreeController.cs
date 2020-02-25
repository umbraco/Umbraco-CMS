using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Packages)]
    [Tree(Constants.Applications.Packages, Constants.Trees.Packages, SortOrder = 0, IsSingleNodeTree = true)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class PackagesTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

        public PackagesTreeController(
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IRuntimeState runtimeState,
            UmbracoHelper umbracoHelper,
            UmbracoMapper umbracoMapper,
            IPublishedUrlProvider publishedUrlProvider,
            IMenuItemCollectionFactory menuItemCollectionFactory)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper, umbracoMapper, publishedUrlProvider)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory;
        }

        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            //this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{Constants.Applications.Packages}/{Constants.Trees.Packages}/repo";
            root.Icon = "icon-box";

            root.HasChildren = false;
            return root;
        }


        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            //full screen app without tree nodes
            return TreeNodeCollection.Empty;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            //doesn't have a menu, this is a full screen app without tree nodes
            return _menuItemCollectionFactory.Create();
        }
    }
}
