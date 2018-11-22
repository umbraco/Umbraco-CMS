using System;
using System.Collections.Generic;
using System.Text;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;


namespace umbraco.cms.presentation.Trees
{
    /// <summary>
    /// Handles loading the content tree into umbraco's application tree
    /// </summary>
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public class ContentRecycleBin : BaseContentTree
    {

        public ContentRecycleBin(string application) : base(application) { }

        protected override bool LoadMinimalDocument
        {
            get
            {
                return true;
            }
        }

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
