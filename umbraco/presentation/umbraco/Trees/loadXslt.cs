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
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Utils;
using umbraco.BusinessLogic.Actions;

namespace umbraco
{
	/// <summary>
	/// Handles loading of the xslt files into the application tree
	/// </summary>
    public class loadXslt : FileSystemTree
	{

        public loadXslt(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {   
			rootNode.NodeType = "init" + TreeAlias;
			rootNode.NodeID = "init";
        }

		/// <summary>
		/// Renders the Javascript
		/// </summary>
		/// <param name="Javascript">The javascript.</param>
        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openXslt(id) {
	parent.right.document.location.href = 'developer/xslt/editXslt.aspx?file=' + id;
}
");
        }
        
        protected override string FilePath
        {
            get { return GlobalSettings.Path + "/../xslt/"; }
        }

        protected override string FileSearchPattern
        {
            get { return "*.xslt"; }
        }

        protected override void OnRenderFileNode(ref XmlTreeNode xNode)
        {
            xNode.Action = xNode.Action.Replace("openFile", "openXslt");
            xNode.Icon = "developerXslt.gif";
            xNode.OpenIcon = "developerXslt.gif";
        }
    }
    
}
