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
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Utils;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core.IO;
using Umbraco.Core;

namespace umbraco
{
	/// <summary>
	/// Handles loading of the xslt files into the application tree
	/// </summary>
    [Tree(Constants.Applications.Developer, "xslt", "XSLT Files", sortOrder: 5)]
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
	UmbClientMgr.contentFrame('developer/xslt/editXslt.aspx?file=' + id);
}
");
        }
        
        protected override string FilePath
        {
            get { return SystemDirectories.Xslt + "/"; }
        }

        protected override string FileSearchPattern
        {
            get { return "*.xslt"; }
        }

        protected override void OnRenderFileNode(ref XmlTreeNode xNode)
        {
            xNode.Action = xNode.Action.Replace("openFile", "openXslt");
            xNode.Icon = "icon-code";
            xNode.OpenIcon = "icon-code";

            xNode.Text = xNode.Text.StripFileExtension();
        }

        protected override void OnRenderFolderNode(ref XmlTreeNode xNode)
        {
            xNode.Menu = new List<IAction>(new IAction[] { ActionDelete.Instance, ContextMenuSeperator.Instance, ActionRefresh.Instance });
            xNode.NodeType = "xsltFolder";
        }
    }
    
}
