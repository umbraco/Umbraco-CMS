using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public abstract class MemberTypeAndGroupTreeControllerBase : TreeController
    {
        public IMenuItemCollectionFactory MenuItemCollectionFactory { get; }

        protected MemberTypeAndGroupTreeControllerBase(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            MenuItemCollectionFactory = menuItemCollectionFactory;
        }

        protected override ActionResult<TreeNodeCollection?> GetTreeNodes(string id, FormCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            nodes.AddRange(GetTreeNodesFromService(id, queryStrings));
            return nodes;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
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
