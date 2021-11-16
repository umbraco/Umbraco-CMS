using System.Collections.Generic;
using System.Net.Http.Formatting;
using Umbraco.Core;
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

            // if the request is for folders only then just return
            if (queryStrings["foldersonly"].IsNullOrWhiteSpace() == false && queryStrings["foldersonly"] == "1") return nodes;

            nodes.AddRange(GetTreeNodesFromService(id, queryStrings));
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.RootString)
            {
                // root actions
                menu.Items.Add(new CreateChildEntity(Services.TextService));
                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }
            else
            {
                var memberType = Services.MemberTypeService.Get(int.Parse(id));
                if (memberType != null)
                {
                    menu.Items.Add<ActionCopy>(Services.TextService, opensDialog: true);
                }

                // delete member type/group
                menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);
            }

            return menu;
        }

        protected abstract IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormDataCollection queryStrings);
    }
}
