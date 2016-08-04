using System;
using umbraco.businesslogic;
using System.Text;
using Umbraco.Core;

namespace umbraco.cms.presentation.Trees
{
    [Tree(Constants.Applications.Users, "userPermissions", "User Permissions", sortOrder: 3)]
    public class UserPermissions : UserGroupPermissionsBaseTree
    {

        public UserPermissions(string application) : base(application) { }

        public override void Render(ref XmlTree tree)
        {
            foreach (var user in umbraco.BusinessLogic.User.getAll())
            {
                if (user.Id > 0 && !user.Disabled)
                {
                    var node = XmlTreeNode.Create(this);
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
            rootNode.Text = ui.Text("user", "userPermissions");
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
