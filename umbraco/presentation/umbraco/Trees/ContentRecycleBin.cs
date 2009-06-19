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
using umbraco.BusinessLogic.Actions;


namespace umbraco.cms.presentation.Trees
{
    /// <summary>
    /// Handles loading the content tree into umbraco's application tree
    /// </summary>
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
            rootNode.Icon = "../tree/bin_empty.png";
			rootNode.OpenIcon = "../tree/bin.png";
            if (new RecycleBin(Document._objectType).Smells())
			{
				rootNode.Icon = "../tree/bin.png";
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
                return -20;
			}
		}

        ///// <summary>
        ///// Returns the tree service url to render the tree. Ensures that IsRecycleBin is flagged.
        ///// </summary>
        ///// <returns></returns>
        //public override string GetTreeInitUrl()
        //{
        //    TreeService treeSvc = new TreeService(true, this.StartNodeID, TreeAlias, null, null, "", "");
        //    return treeSvc.GetInitUrl();
        //}

        protected override void OnRenderNode(ref XmlTreeNode xNode, Document doc)
        {
			xNode.DimNode();
        }

    }
}
