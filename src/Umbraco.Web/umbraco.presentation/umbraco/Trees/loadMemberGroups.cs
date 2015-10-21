using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
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
using System.Web.Security;
using Umbraco.Core;

namespace umbraco
{
	/// <summary>
	/// Handles loading of the member groups into the application tree
	/// </summary>
    [Tree(Constants.Applications.Members, "memberGroups", "Member Groups", sortOrder: 1)]
    public class loadMemberGroups : BaseTree
	{
        public loadMemberGroups(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            // if we're using 3rd party membership providers we should use the Role terminology
            if (!Member.IsUsingUmbracoRoles())
            {
                rootNode.Text = ui.Text("memberRoles");
            }
            rootNode.NodeType = "initmemberGroup";
            rootNode.NodeID = "init";
        }

		/// <summary>
		/// Renders the Javascript.
		/// </summary>
		/// <param name="Javascript">The javascript.</param>
		public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openMemberGroup(id) {
	UmbClientMgr.contentFrame('members/editMemberGroup.aspx?id=' + id);
}
");
        }

        public override void Render(ref XmlTree tree)
        {
            var roles = Roles.GetAllRoles();
            Array.Sort(roles);

            foreach(string role in roles) {
                if (role.StartsWith(Constants.Conventions.Member.InternalRolePrefix) == false)
                {
                    XmlTreeNode xNode = XmlTreeNode.Create(this);
                    xNode.NodeID = role;
                    xNode.Text = role;
                    xNode.Action = "javascript:openMemberGroup('" + HttpContext.Current.Server.UrlEncode(role.Replace("'", "\\'")) + "');";
                    xNode.Icon = "icon-users";
                    if (!Member.IsUsingUmbracoRoles())
                    {
                        xNode.Menu = null;
                    }

                    OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    if (xNode != null)
                    {
                        tree.Add(xNode);
                    }
                    OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                }
            }
        }
	}
}
