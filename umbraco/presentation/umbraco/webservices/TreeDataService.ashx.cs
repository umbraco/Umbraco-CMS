using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using umbraco.cms.presentation.Trees;

namespace umbraco.presentation.webservices
{
	/// <summary>
	/// Summary description for TreeDataService
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	public class TreeDataService : IHttpHandler
	{

		public void ProcessRequest(HttpContext context)
		{

			context.Response.ContentType = "application/json";
			context.Response.Write(GetXmlTree().ToString());

		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Returns the an XmlTree based on the current http request
		/// </summary>
		/// <returns></returns>
		public XmlTree GetXmlTree()
		{
			TreeRequestParams treeParams = TreeRequestParams.FromQueryStrings();

			if (string.IsNullOrEmpty(treeParams.TreeType))
				if (!string.IsNullOrEmpty(treeParams.Application))
					LoadAppTrees(treeParams);
				else
					return new XmlTree(); //returns an empty tree
			else
				LoadTree(treeParams);

			return xTree;
		}

		private XmlTree xTree = new XmlTree();

		/// <summary>
		/// If the application supports multiple trees, then this function iterates over all of the trees assigned to it
		/// and creates their top level nodes and context menus.
		/// </summary>
		/// <param name="appAlias"></param>
		private void LoadAppTrees(TreeRequestParams treeParams)
		{
			//find all tree definitions that have the current application alias
			List<TreeDefinition> treeDefs = TreeDefinitionCollection.Instance.FindActiveTrees(treeParams.Application);

			foreach (TreeDefinition treeDef in treeDefs)
			{
				BaseTree bTree = treeDef.CreateInstance();
				bTree.SetTreeParameters(treeParams);
				xTree.Add(bTree.RootNode);
			}
		}

		/// <summary>
		/// This will load the particular ITree object and call it's render method to get the nodes that need to be rendered.
		/// </summary>
		/// <param name="appAlias"></param>
		/// <param name="treeAlias"></param>
		private void LoadTree(TreeRequestParams treeParams)
		{

			TreeDefinition treeDef = TreeDefinitionCollection.Instance.FindTree(treeParams.TreeType);

			if (treeDef != null)
			{
				BaseTree bTree = treeDef.CreateInstance();
				bTree.SetTreeParameters(treeParams);
				bTree.Render(ref xTree);
			}
			else
				LoadNullTree(treeParams);
		}

		/// <summary>
		/// Load an empty tree structure to show the end user that there was a problem loading the tree.
		/// </summary>
		private void LoadNullTree(TreeRequestParams treeParams)
		{
			BaseTree nullTree = new NullTree(treeParams.Application);
			nullTree.SetTreeParameters(treeParams);
			nullTree.Render(ref xTree);
		}
	}
}
