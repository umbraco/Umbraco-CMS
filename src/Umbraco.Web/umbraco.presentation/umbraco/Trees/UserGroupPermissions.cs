using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using umbraco.businesslogic;
using umbraco.interfaces;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;

namespace umbraco.cms.presentation.Trees
{
    [Tree(Constants.Applications.Users, "UserGroupPermissions", "User Group Permissions", sortOrder: 3)]
    public class UserGroupPermissions : BaseTree
    {

        public UserGroupPermissions(string application) : base(application) { }

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
            var userService = ApplicationContext.Current.Services.UserService;
            foreach (var group in userService.GetAllUserGroups())
            {
                var node = XmlTreeNode.Create(this);
                node.NodeID = group.Id.ToString();
                node.Text = group.Name;
                node.Action = "javascript:openUserGroupPermissions('" + group.Id.ToString() + "');";
                node.Icon = "icon-users";

                OnBeforeNodeRender(ref tree, ref node, EventArgs.Empty);
                if (node != null)
                {
                    tree.Add(node);
                    OnAfterNodeRender(ref tree, ref node, EventArgs.Empty);
                }
            }
        }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.Text = ui.Text("user", "userGroupPermissions");
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openUserGroupPermissions(id) {
	UmbClientMgr.contentFrame('users/PermissionEditor.aspx?id=' + id);
}
");
        }

    }
}
