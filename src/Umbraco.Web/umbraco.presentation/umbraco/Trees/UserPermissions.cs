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
            foreach (umbraco.BusinessLogic.User user in umbraco.BusinessLogic.User.getAll())
            {
                if (user.Id > 0 && !user.Disabled)
                {
                    XmlTreeNode node = XmlTreeNode.Create(this);
                    node.NodeID = user.Id.ToString();
                    node.Text = user.Name;
                    node.Action = "javascript:openUserPermissions('" + user.Id.ToString() + "');";
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
