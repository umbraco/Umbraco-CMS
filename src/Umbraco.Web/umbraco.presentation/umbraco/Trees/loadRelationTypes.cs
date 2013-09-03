using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.presentation.Trees;
using umbraco.DataLayer;
using umbraco.interfaces;
using umbraco.cms.presentation.developer.RelationTypes.TreeMenu;
using umbraco.businesslogic;
using Umbraco.Core;

namespace umbraco
{
	/// <summary>
	/// RelationTypes tree for developer section
	/// </summary>
	[Tree(Constants.Applications.Developer, "relationTypes", "Relation Types", sortOrder: 4)]
	public class loadRelationTypes : BaseTree
	{
		/// <summary>
		/// Initializes a new instance of the RelationTypeTree class.
		/// </summary>
		/// <param name="application">name of umbraco app to which this tree has been added, (in this case "developer")</param>
		public loadRelationTypes(string application)
			: base(application)
		{
		}

		/// <summary>
		/// Builds the javascript methods for use by the nodes in this tree
		/// </summary>
		/// <param name="javascript">string container for javascript</param>
		public override void RenderJS(ref StringBuilder javascript)
		{
			javascript.Append(@"
				function openRelationType(id) {
					UmbClientMgr.contentFrame('developer/RelationTypes/EditRelationType.aspx?id=' + id);
				}");
		}

		/// <summary>
		/// This is called if the tree has been expanded, and it's used to render and child nodes for this tree
		/// </summary>
		/// <param name="tree">current tree</param>
		public override void Render(ref XmlTree tree)
		{
			XmlTreeNode node;

			foreach (RelationType relationType in RelationType.GetAll().OrderBy(relationType => relationType.Name))
			{
				node = XmlTreeNode.Create(this);
				node.NodeID = relationType.Id.ToString();
				node.Text = relationType.Name;
				node.Icon = "icon-trafic";
				node.Action = string.Concat("javascript:openRelationType('", node.NodeID, "');");

				tree.Add(node);
			}
		}

		/// <summary>
		/// Adds right click context tree actions for each child node
		/// </summary>
		/// <param name="actions">collection of actions (expected to be empty)</param>
		protected override void CreateAllowedActions(ref List<IAction> actions)
		{
			actions.Clear();
			actions.Add(ActionDeleteRelationType.Instance);
		}

		/// <summary>
		/// Adds right click context tree actions for the root 'Relation Types' node
		/// </summary>
		/// <param name="actions">collection of actions (expected to be empty)</param>
		protected override void CreateRootNodeActions(ref List<IAction> actions)
		{
			actions.Clear();
			actions.Add(ActionNewRelationType.Instance);
			actions.Add(ContextMenuSeperator.Instance);
			actions.Add(ActionRefresh.Instance);
		}

		/// <summary>
		/// Configures root node 'Relation Types' properties
		/// </summary>
		/// <param name="rootNode">the 'Relation Types' root node</param>
		protected override void CreateRootNode(ref XmlTreeNode rootNode)
		{
			rootNode.Text = "Relation Types";
			rootNode.Icon = BaseTree.FolderIcon;
			rootNode.OpenIcon = BaseTree.FolderIconOpen;
			rootNode.NodeType = this.TreeAlias; // (Was prefixed with init)
			rootNode.NodeID = "init";
		}
	}
}
