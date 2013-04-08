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

namespace umbraco
{
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
	public class loadcontentItemType : BaseTree
	{
        public loadcontentItemType(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {           
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

		public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openContentItemType(id) {
	UmbClientMgr.contentFrame('settings/editContentItemType.aspx?id=' + id);
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

		public override void Render(ref XmlDocument Tree)
        {
            XmlNode root = Tree.DocumentElement;

            foreach (ContentItemType dt in ContentItemType.GetAll)
            {
                XmlElement treeElement = Tree.CreateElement("tree");
                treeElement.SetAttribute("nodeID", dt.Id.ToString());
                treeElement.SetAttribute("menu", "D");
                treeElement.SetAttribute("text", dt.Text);
                treeElement.SetAttribute("action", "javascript:openContentItemType(" + dt.Id + ");");
                treeElement.SetAttribute("src", "");
                treeElement.SetAttribute("icon", "settingDataType.gif");
                treeElement.SetAttribute("openIcon", "settingDataType.gif");
                treeElement.SetAttribute("nodeType", "contentItemType");
                root.AppendChild(treeElement);
            }
        }
	}
    
}
