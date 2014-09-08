using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using umbraco.businesslogic;
using Umbraco.Core;

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
            List<UserType> userTypes = UserType.GetAllUserTypes();
            foreach (UserType userType in userTypes)
            {
                if (userType.Id > 1) //don't show the admin user type, they should always have full permissions
                {
                    XmlTreeNode node = XmlTreeNode.Create(this);
                    node.NodeID = userType.Id.ToString();
                    node.Action = string.Format("javascript:openUserTypes({0})", userType.Id.ToString());
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
            rootNode.Text = ui.Text("user", "userTypes");
        }
    }
}
