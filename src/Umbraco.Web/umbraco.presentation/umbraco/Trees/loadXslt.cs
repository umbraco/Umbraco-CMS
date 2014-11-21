using System.Collections.Generic;
using System.Text;
using umbraco.businesslogic;
using umbraco.interfaces;
using umbraco.cms.presentation.Trees;
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
