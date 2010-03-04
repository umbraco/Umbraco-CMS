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
    public class loadMediaTypes : BaseTree
    {
        public loadMediaTypes(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openMediaType(id) {
	parent.right.document.location.href = 'settings/editMediaType.aspx?id=' + id;
}
");
        }

        public override void Render(ref XmlTree tree)
        {
            foreach (MediaType dt in MediaType.GetAll)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = dt.Id.ToString();
                xNode.Text = dt.Text;
                xNode.Action = string.Format("javascript:openMediaType({0});", dt.Id);
                xNode.Icon = "settingDataType.gif";
                xNode.OpenIcon = "settingDataType.gif";

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
