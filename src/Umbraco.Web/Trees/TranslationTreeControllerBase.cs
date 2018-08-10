using System.Net.Http.Formatting;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    public enum TranslationTaskUserType
    {
        Assignee,
        Owner
    }

    public abstract class TranslationTreeTreeControllerBase : TreeController
    {
        private readonly TranslationTaskUserType taskUserType;

        internal const string BASE_ROUTE = "/translation/translation";

        public TranslationTreeTreeControllerBase(TranslationTaskUserType type)
        {
            taskUserType = type;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            var tasks = taskUserType == TranslationTaskUserType.Assignee ?
                Services.TaskService.GetTasks(assignedUser: Security.CurrentUser.Id, taskTypeAlias: "toTranslate") :
                Services.TaskService.GetTasks(ownerUser: Security.CurrentUser.Id, taskTypeAlias: "toTranslate");

            foreach (var task in tasks)
            {
                var entity = Services.ContentService.GetById(task.EntityId);

                var node = CreateTreeNode(task.Id.ToString(),
                    id,
                    null,
                    entity.Name,
                    "icon-document-dashed-line",
                    false,
                    BASE_ROUTE + taskUserType.ToString().ToLower().EnsureStartsWith('/') + task.Id.ToString().EnsureStartsWith('/'));

                nodes.Add(node);
            }

            return nodes;
        }

        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var node = base.CreateRootNode(queryStrings);

            node.RoutePath = BASE_ROUTE + "tasks".EnsureStartsWith('/') + taskUserType.ToString().ToLower().EnsureStartsWith('/');

            return node;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);

            return menu;
        }
    }
}
