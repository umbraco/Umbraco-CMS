using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using Umbraco.Core.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.contentitem;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;
using System.Linq;
using Umbraco.Core;


namespace umbraco
{
	/// <summary>
	/// Handles loading of all umbraco users into the users application tree
	/// </summary>
    [Tree(Constants.Applications.Users, "users", "Users")]
    public class loadUsers : BaseTree
	{
        public loadUsers(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {                        
        }

		/// <summary>
		/// Renders the Javascript.
		/// </summary>
		/// <param name="Javascript">The javascript.</param>
		public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openUser(id) {
	UmbClientMgr.contentFrame('users/editUser.aspx?id=' + id);
}
");
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionDisable.Instance);
        }

        public override void Render(ref XmlTree tree)
        {
            var users = new List<User>(User.getAll());

            User currUser = UmbracoEnsuredPage.CurrentUser;

            bool currUserIsAdmin = currUser.IsAdmin();
            foreach (User u in users.OrderBy(x => x.Disabled))
            {
                if (!UmbracoConfig.For.UmbracoSettings().Security.HideDisabledUsersInBackoffice
                    || (UmbracoConfig.For.UmbracoSettings().Security.HideDisabledUsersInBackoffice && !u.Disabled))
                {

                    XmlTreeNode xNode = XmlTreeNode.Create(this);

                    // special check for ROOT user
                    if (u.Id == 0)
                    {
                        //if its the administrator, don't create a menu
                        xNode.Menu = null;
                        //if the current user is not the administrator, then don't add this node.
                        if (currUser.Id != 0)
                            continue;
                    }
                    // Special check for admins in general (only show admins to admins)
                    else if (!currUserIsAdmin && u.IsAdmin())
                    {
                        continue;
                    }





                    //xNode.IconClass = "umbraco-tree-icon-grey";

                    xNode.NodeID = u.Id.ToString();
                    xNode.Text = u.Name;
                    xNode.Action = "javascript:openUser(" + u.Id + ");";
                    xNode.Icon = "icon-user";
                    xNode.OpenIcon = "icon-user";

                    if (u.Disabled) {
                        xNode.Style.DimNode();
                    }

                    OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    if (xNode != null)
                    {
                        tree.Add(xNode);
                        OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    }
                

                }
                
                
            }
        }

	}
    
}
