using System.Collections.Generic;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web._Legacy.Actions;
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
                menu.Items.Add<CreateChildEntity, ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias));
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }
            else
            {
                //delete member type/group
                menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias));
            }

            return menu;
        }

        protected abstract IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormDataCollection queryStrings);
    }
}
