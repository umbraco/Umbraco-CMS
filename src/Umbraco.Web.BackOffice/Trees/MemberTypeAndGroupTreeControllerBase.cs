using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.BackOffice.Trees;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Trees
{
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public abstract class MemberTypeAndGroupTreeControllerBase : TreeController
    {
        public IMenuItemCollectionFactory MenuItemCollectionFactory { get; }

        protected MemberTypeAndGroupTreeControllerBase(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory)
            : base(localizedTextService, umbracoApiControllerTypeCollection)
        {
            MenuItemCollectionFactory = menuItemCollectionFactory;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            nodes.AddRange(GetTreeNodesFromService(id, queryStrings));
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            if (id == Constants.System.RootString)
            {
                // root actions
                menu.Items.Add(new CreateChildEntity(LocalizedTextService));
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                return menu;
            }
            else
            {
                //delete member type/group
                menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);
            }

            return menu;
        }

        protected abstract IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormCollection queryStrings);
    }
}
