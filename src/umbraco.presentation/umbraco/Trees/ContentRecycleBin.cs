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
using umbraco.BusinessLogic.Actions;


namespace umbraco.cms.presentation.Trees
{
    /// <summary>
    /// Handles loading the content tree into umbraco's application tree
    /// </summary>
    [Tree("content", "contentRecycleBin", "Recycle Bin", "folder.gif", "folder_o.gif", initialize: false)]
    public class ContentRecycleBin : BaseContentTree
    {

        public ContentRecycleBin(string application) : base(application) { }

        protected override void CreateRootNodeActions(ref List<IAction> actions)
		{
			actions.Clear();
            actions.Add(ActionEmptyTranscan.Instance);
			actions.Add(ContextMenuSeperator.Instance);
			actions.Add(ActionRefresh.Instance);
		}

        protected override void CreateAllowedActions(ref List<IAction> actions)
		{
            actions.Clear();
            actions.Add(ActionMove.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionDelete.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionRefresh.Instance);
		}

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.Icon = "bin_empty.png";
			rootNode.OpenIcon = "bin.png";
			//we made this alias the same as content, so the node name would be media,
			//we need to make it recycle bin
			TreeDefinition treeDef = TreeDefinitionCollection.Instance.FindTree(this);
			rootNode.Text = rootNode.Text = GetTreeHeader(treeDef.Tree.Alias);

            if (new RecycleBin(RecycleBin.RecycleBinType.Content).Smells())
			{
				rootNode.Icon = "bin.png";
			}				
            else
                rootNode.Source = null;
        }
      

        /// <summary>
        /// By default the Recycle bin start node is -20
        /// </summary>
		public override int StartNodeID
		{
			get
			{
                return (int)RecycleBin.RecycleBinType.Content;
			}
		}

        /// <summary>
        /// Override the render js so no duplicate js is rendered.
        /// </summary>
        /// <param name="Javascript"></param>
        public override void RenderJS(ref StringBuilder Javascript) { }
      
        protected override void OnRenderNode(ref XmlTreeNode xNode, Document doc)
        {
			xNode.Style.DimNode();
        }

    }
}
