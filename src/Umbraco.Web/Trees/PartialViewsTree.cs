using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.template;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;

namespace Umbraco.Web.Trees
{
	/// <summary>
	/// Tree for displaying partial views in the settings app
	/// </summary>
	[Tree(Constants.Applications.Settings, "partialViews", "Partial Views", sortOrder: 2)]
	public class PartialViewsTree : FileSystemTree
	{
		public PartialViewsTree(string application) : base(application) { }

		public override void RenderJS(ref StringBuilder javascript)
		{
			javascript.Append(
                @"
		                 function openPartialView(id) {
		                    UmbClientMgr.contentFrame('Settings/Views/EditView.aspx?treeType=partialViews&file=' + id);
					    }
		                ");
		}

		protected override void CreateRootNode(ref XmlTreeNode rootNode)
		{
			rootNode.NodeType = TreeAlias;
			rootNode.NodeID = "init";
		}

		protected override string FilePath
		{
			get { return SystemDirectories.MvcViews + "/Partials/"; }
		}

		protected override string FileSearchPattern
		{
			get { return "*.cshtml"; }
		}

		/// <summary>
		/// Ensures that no folders can be added
		/// </summary>
		/// <param name="xNode"></param>
		protected override void OnRenderFolderNode(ref XmlTreeNode xNode)
		{
            // We should allow folder hierarchy for organization in large sites.
            xNode.Action = "javascript:void(0);";
            xNode.NodeType = "partialViewsFolder";
            xNode.Menu = new List<IAction>(new IAction[]
            {
                ActionNew.Instance, 
                ContextMenuSeperator.Instance, 
                ActionDelete.Instance, 
                ContextMenuSeperator.Instance, 
                ActionRefresh.Instance
            });
            
		}

		protected virtual void ChangeNodeAction(XmlTreeNode xNode)
		{
			xNode.Action = xNode.Action.Replace("openFile", "openPartialView");
		}

		protected override void OnRenderFileNode(ref XmlTreeNode xNode)
		{
			ChangeNodeAction(xNode);
			xNode.Icon = "icon-article";
			xNode.OpenIcon = "icon-article";

            xNode.Text = xNode.Text.StripFileExtension();
		}

		
	}
}
