using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Tree for displaying partial view macros in the developer app
    /// </summary>
    [Tree(Constants.Applications.Developer, "partialViewMacros", "Partial View Macro Files", sortOrder: 6)]
	public class PartialViewMacrosTreeController : FileSystemTreeController
    {
        protected override string FilePath
        {
            get { return SystemDirectories.MacroPartials; }
        }

        protected override string FileSearchPattern
        {
            get { return "*.cshtml"; }
        }

        protected override string FileIcon
        {
            get { return "icon-article"; }
        }

        protected override void OnRenderFileNode(ref TreeNode treeNode)
        {
            base.OnRenderFileNode(ref treeNode);
        }

        protected override void OnRenderFolderNode(ref TreeNode treeNode)
        {
            //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
            treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
        }
    }
}
