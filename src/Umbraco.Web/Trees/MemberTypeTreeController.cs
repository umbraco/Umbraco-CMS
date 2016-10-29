using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.MemberTypes)]
    [Tree(Constants.Applications.Members, Constants.Trees.MemberTypes, null, sortOrder:2  )]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    [LegacyBaseTree(typeof(loadMemberTypes))]
    public class MemberTypeTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            nodes.AddRange(
                Services.MemberTypeService.GetAll()
                    .OrderBy(x => x.Name)
                    .Select(dt => CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, "icon-item-arrangement", false)));
            
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions              
                menu.Items.Add<CreateChildEntity, ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }
            else
            {
                //delete member type
                menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));
            }

            return menu;
        }
    }
}
