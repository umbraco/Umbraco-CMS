using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.Configuration;
using Umbraco.Core.Security;
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
using Umbraco.Core;


namespace umbraco
{
	/// <summary>
	/// Handles loading of the member types into the application tree
	/// </summary>
    [Tree(Constants.Applications.Members, "memberType", "Member Types", sortOrder: 2)]
    public class loadMemberTypes : BaseTree
	{
        public loadMemberTypes(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
 
            // only show member types if we're using umbraco members on the website
            if (provider.IsUmbracoMembershipProvider())
            {
				rootNode.NodeType = "init" + TreeAlias;
				rootNode.NodeID = "init";
            }
            else
            {
                rootNode = null;
            }
        }

		/// <summary>
		/// Renders the Javascript.
		/// </summary>
		/// <param name="Javascript">The javascript.</param>
		public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openMemberType(id) {
	UmbClientMgr.contentFrame('members/editMemberType.aspx?id=' + id);
}
");
        }

        /// <summary>
        /// This will call the normal Render method by passing the converted XmlTree to an XmlDocument.
        /// TODO: need to update this render method to do everything that the obsolete render method does and remove the obsolete method
        /// </summary>
        /// <param name="tree"></param>
        public override void Render(ref XmlTree tree)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(tree.ToString(SerializedTreeType.XmlTree));
            Render(ref xDoc);
            tree = SerializableData.Deserialize(xDoc.OuterXml, typeof(XmlTree)) as XmlTree;
			//ensure that the tree type is set! this wouldn't need to be done if BaseTree was implemented properly
			foreach (XmlTreeNode node in tree)
				node.TreeType = this.TreeAlias;
        }

		/// <summary>
		/// Renders the specified tree item.
		/// </summary>
		/// <param name="Tree">The tree.</param>
		public override void Render(ref XmlDocument Tree)
        {
            MemberType[] MemberTypes = MemberType.GetAll;
            XmlNode root = Tree.DocumentElement;
            for (int i = 0; i < MemberTypes.Length; i++)
            {
                XmlElement treeElement = Tree.CreateElement("tree");
                treeElement.SetAttribute("menu", "D");
                treeElement.SetAttribute("nodeID", MemberTypes[i].Id.ToString());
                treeElement.SetAttribute("text", MemberTypes[i].Text);
                treeElement.SetAttribute("action", "javascript:openMemberType(" + MemberTypes[i].Id + ");");
                treeElement.SetAttribute("src", "");
                treeElement.SetAttribute("icon", "icon-users");
                treeElement.SetAttribute("openIcon", "icon-users");
                treeElement.SetAttribute("nodeType", "memberType");
                root.AppendChild(treeElement);
            }
        }
	}
    
}
