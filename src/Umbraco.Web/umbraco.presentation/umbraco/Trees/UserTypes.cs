using Umbraco.Core.Services;
using System;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Web.Trees;

namespace umbraco.cms.presentation.Trees
{
    [Tree(Constants.Applications.Users, "userTypes", "User Types", sortOrder: 1)]
    public class UserTypes : BaseTree
    {

        public UserTypes(string application) : base(application) { }

        public override void RenderJS(ref System.Text.StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openUserTypes(id) {
	UmbClientMgr.contentFrame('users/EditUserType.aspx?id=' + id);
}
");
        }

        public override void Render(ref XmlTree tree)
        {
            var userTypes = Services.UserService.GetAllUserTypes();
            foreach (var userType in userTypes)
            {
                if (userType.Id > 1) //don't show the admin user type, they should always have full permissions
                {
                    XmlTreeNode node = XmlTreeNode.Create(this);
                    node.NodeID = userType.Id.ToString();
                    node.Action = string.Format("javascript:openUserTypes({0})", userType.Id);
                    node.Icon = "icon-users";
                    node.Text = userType.Name;

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
            rootNode.Text = Services.TextService.Localize("user/userTypes");
        }
    }
}
