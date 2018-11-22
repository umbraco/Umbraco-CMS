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
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;


namespace umbraco
{
	/// <summary>
	/// Handles loading of all datatypes into the developer application tree
	/// </summary>    
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public class loadDataTypes : BaseTree
	{
        public loadDataTypes(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {   
            rootNode.NodeType = "init" + TreeAlias;
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
function openDataType(id) {
	UmbClientMgr.contentFrame('developer/datatypes/editDataType.aspx?id=' + id);
}
");
        }

        public override void Render(ref XmlTree tree)
        {
            foreach (DataTypeDefinition dt in DataTypeDefinition.GetAll())
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = dt.Id.ToString();
                xNode.Text = dt.Text;
                xNode.Action = "javascript:openDataType(" + dt.Id + ");";
                xNode.Icon = "icon-autofill";
                xNode.OpenIcon = "icon-autofill";

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
