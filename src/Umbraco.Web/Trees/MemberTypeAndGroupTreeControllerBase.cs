using System.Collections.Generic;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;

using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public abstract class MemberTypeAndGroupTreeControllerBase : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            nodes.AddRange(GetTreeNodesFromService(id, queryStrings));
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
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
