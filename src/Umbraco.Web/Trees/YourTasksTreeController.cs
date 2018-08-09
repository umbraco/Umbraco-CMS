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
    [Tree(Constants.Applications.Translation, "yourTasks", null, sortOrder: 2)]
    [LegacyBaseTree(typeof(loadYourTasks))]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class YourTasksTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            foreach (var task in Services.TaskService.GetTasks(assignedUser: Security.CurrentUser.Id))
            {
                var entity = Services.ContentService.GetById(task.EntityId);

                var node = CreateTreeNode(task.Id.ToString(),
                    id,
                    queryStrings,
                    entity.Name,
                    "icon-document-dashed-line",
                    false,
                    // Whi twice translation?
                    queryStrings.GetValue<string>("application").EnsureStartsWith('/') + "tasks".EnsureStartsWith('/') + "owned".EnsureStartsWith('/') + task.Id.ToString().EnsureStartsWith('/'));

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
