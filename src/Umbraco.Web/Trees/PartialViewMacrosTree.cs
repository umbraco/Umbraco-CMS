using System.Text;
using Umbraco.Core.IO;
using umbraco.businesslogic;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;

namespace Umbraco.Web.Trees
{
	/// <summary>
	/// Tree for displaying partial view macros in the developer app
	/// </summary>
	[Tree(Constants.Applications.Developer, "partialViewMacros", "Partial View Macro Files", sortOrder: 6)]
	public class PartialViewMacrosTree : PartialViewsTree
	{
		public PartialViewMacrosTree(string application) : base(application)
		{
		}

		protected override string FilePath
		{
			get { return SystemDirectories.MvcViews + "/MacroPartials/"; }
		}

		public override void RenderJS(ref StringBuilder javascript)
		{
			javascript.Append(
                @"
		                 function openMacroPartialView(id) {
		                    UmbClientMgr.contentFrame('Settings/Views/EditView.aspx?treeType=partialViewMacros&file=' + id);
					    }
		                ");

		}/// <summary>
		/// Ensures that no folders can be added
		/// </summary>
		/// <param name="xNode"></param>
        protected override void OnRenderFolderNode(ref XmlTreeNode xNode)
        {
            base.OnRenderFolderNode(ref xNode);

            xNode.NodeType = "partialViewMacrosFolder";
        }

		protected override void ChangeNodeAction(XmlTreeNode xNode)
		{
			xNode.Action = xNode.Action.Replace("openFile", "openMacroPartialView");
		}
	}
}