using Umbraco.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core;
using Umbraco.Web.Trees;
using Umbraco.Web._Legacy.Actions;

namespace umbraco.cms.presentation.Trees
{
    [Tree(Constants.Applications.Users, "userPermissions", "User Permissions", sortOrder: 2)]
    public class UserPermissions : BaseTree
    {

        public UserPermissions(string application) : base(application) { }

        /// <summary>
        /// don't allow any actions on this tree
        /// </summary>
        /// <param name="actions"></param>
        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
        }

        /// <summary>
        /// no actions should be able to be performed on the parent node except for refresh
        /// </summary>
        /// <param name="actions"></param>
        protected override void CreateRootNodeActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionRefresh.Instance);
        }

        public override void Render(ref XmlTree tree)
        {
            long totalusers;
            foreach (var user in Services.UserService.GetAll(0, int.MaxValue, out totalusers))
            {
                if (user.Id > 0 && user.IsApproved)
                {
                    XmlTreeNode node = XmlTreeNode.Create(this);
                    node.NodeID = user.Id.ToString();
                    node.Text = user.Name;
                    node.Action = "javascript:openUserPermissions('" + user.Id + "');";
                    node.Icon = "icon-users";

                    OnBeforeNodeRender(ref tree, ref node, EventArgs.Empty);
                    if (node != null)
                    {
                        tree.Add(node);
                        OnAfterNodeRender(ref tree, ref node, EventArgs.Empty);
                    }
                    
                }
            }
        }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.Text = Services.TextService.Localize("user/userPermissions");
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openUserPermissions(id) {
	UmbClientMgr.contentFrame('users/PermissionEditor.aspx?id=' + id);
}
");
        }

    }
}
