using System;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using umbraco.businesslogic;
using Umbraco.Core;

namespace umbraco.cms.presentation.Trees
{
    [Tree(Constants.Applications.Users, "userGroups", "User Groups", sortOrder: 2)]
    public class UserGroups : BaseTree
    {

        public UserGroups(string application) : base(application) { }

        public override void RenderJS(ref System.Text.StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openUserGroups(id) {
	UmbClientMgr.contentFrame('users/EditUserGroup.aspx?id=' + id);
}
");
        }

        public override void Render(ref XmlTree tree)
        {
            var userGroups = ApplicationContext.Current.Services.UserService.GetAllUserGroups();
            foreach (var userGroup in userGroups)
            {
                XmlTreeNode node = XmlTreeNode.Create(this);
                node.NodeID = userGroup.Id.ToString();
                node.Action = string.Format("javascript:openUserGroups({0})", userGroup.Id);
                node.Icon = "icon-users";
                node.Text = userGroup.Name;

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
            rootNode.Text = ui.Text("user", "userGroups");
        }
    }
}