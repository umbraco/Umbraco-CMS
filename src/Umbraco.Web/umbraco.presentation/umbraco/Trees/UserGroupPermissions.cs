using System;
using umbraco.businesslogic;
using System.Text;
using Umbraco.Core;

namespace umbraco.cms.presentation.Trees
{
    [Tree(Constants.Applications.Users, "userGroupPermissions", "User Group Permissions", sortOrder: 4)]
    public class UserGroupPermissions : UserGroupPermissionsBaseTree
    {
        public UserGroupPermissions(string application) : base(application) { }

        public override void Render(ref XmlTree tree)
        {
            var userService = ApplicationContext.Current.Services.UserService;
            foreach (var group in userService.GetAllUserGroups())
            {
                var node = XmlTreeNode.Create(this);
                node.NodeID = group.Id.ToString();
                node.Text = group.Name;
                node.Action = "javascript:openUserGroupPermissions('" + group.Id + "');";
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
	UmbClientMgr.contentFrame('users/GroupPermissionEditor.aspx?id=' + id);
}
");
        }

    }
}
