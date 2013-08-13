using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Linq;
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
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;


namespace umbraco.cms.presentation.Trees
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
	public class MediaRecycleBin : BaseMediaTree
	{
		public MediaRecycleBin(string application) : base(application) { }

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

		/// <summary>
		/// By default the Recycle bin start node is -21
		/// </summary>
		public override int StartNodeID
		{
			get
			{
				return (int)RecycleBin.RecycleBinType.Media;
			}
		}

        /// <summary>
        /// Override the render js so no duplicate js is rendered.
        /// </summary>
        /// <param name="Javascript"></param>
        public override void RenderJS(ref StringBuilder Javascript) { }

		protected override void CreateRootNode(ref XmlTreeNode rootNode)
		{
			rootNode.Icon = "bin_empty.png";
			rootNode.OpenIcon = "bin.png";
			//we made this alias the same as media, so the node name would be media,
			//we need to make it recycle bin
			TreeDefinition treeDef = TreeDefinitionCollection.Instance.FindTree(this);
			rootNode.Text = rootNode.Text = GetTreeHeader(treeDef.Tree.Alias);
			if (new RecycleBin(Media._objectType, RecycleBin.RecycleBinType.Media).Smells())
			{
				rootNode.Icon = "bin.png";
			}
			else
				rootNode.Source = null;
		}

		protected override void OnBeforeNodeRender(ref XmlTree sender, ref XmlTreeNode node, EventArgs e)
		{
			base.OnBeforeNodeRender(ref sender, ref node, e);
			node.Style.DimNode();
		}

	}
}
