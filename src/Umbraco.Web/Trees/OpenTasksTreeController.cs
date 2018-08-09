using System.Net.Http.Formatting;
using umbraco;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Translations)]
    [Tree(Constants.Applications.Translation, "openTasks", null, sortOrder: 1)]
    [LegacyBaseTree(typeof(loadOpenTasks))]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class OpenTasksTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            foreach (var task in Services.TaskService.GetTasks(assignedUser: Security.CurrentUser.Id))
            {
                var entity = Services.ContentService.GetById(task.EntityId);
                
                var node = CreateTreeNode(task.Id.ToString(),
                    id,
                    null,
                    entity.Name,
                    "icon-document-dashed-line",
                    false,
                    queryStrings.GetValue<string>("application") + queryStrings.GetValue<string>("application").EnsureStartsWith('/') + TreeAlias.EnsureStartsWith('/') + task.Id.ToString().EnsureStartsWith('/'));

                nodes.Add(node);
            }

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return null;
        }
    }
}
