using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.MemberGroups)]
    [Tree(Constants.Applications.Members, Constants.Trees.MemberGroups, SortOrder = 1)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public class MemberGroupTreeController : MemberTypeAndGroupTreeControllerBase
    {
        protected override IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormDataCollection queryStrings)
        {
            return Services.MemberGroupService.GetAll()
                .OrderBy(x => x.Name)
                .Select(dt => CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, "icon-item-arrangement", false));
        }

        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            //check if there are any groups
            root.HasChildren = Services.MemberGroupService.GetAll().Any();
            return root;
        }
    }
}
