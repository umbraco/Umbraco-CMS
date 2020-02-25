using System.Collections.Generic;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Trees
{
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public abstract class MemberTypeAndGroupTreeControllerBase : TreeController
    {
        public IMenuItemCollectionFactory MenuItemCollectionFactory { get; }

        protected MemberTypeAndGroupTreeControllerBase()
        {
            MenuItemCollectionFactory = Current.MenuItemCollectionFactory;
        }

        protected MemberTypeAndGroupTreeControllerBase(
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
            MenuItemCollectionFactory = menuItemCollectionFactory;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            nodes.AddRange(GetTreeNodesFromService(id, queryStrings));
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            if (id == Constants.System.RootString)
            {
                // root actions
                menu.Items.Add(new CreateChildEntity(Services.TextService));
                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }
            else
            {
                //delete member type/group
                menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);
            }

            return menu;
        }

        protected abstract IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormDataCollection queryStrings);
    }
}
