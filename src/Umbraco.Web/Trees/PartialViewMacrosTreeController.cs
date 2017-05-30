using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
	/// <summary>
	/// Tree for displaying partial view macros in the developer app
	/// </summary>
	[Tree(Constants.Applications.Developer, Constants.Trees.PartialViewMacros, "Partial View Macro Files", sortOrder: 6)]
	public class PartialViewMacrosTreeController : PartialViewsTreeController
	{
	    protected override IFileSystem FileSystem => Current.FileSystems.MacroPartialsFileSystem;

	    private static readonly string[] ExtensionsStatic = { "cshtml" };

	    protected override string[] Extensions => ExtensionsStatic;

	    protected override string FileIcon => "icon-article";

	    protected override void OnRenderFolderNode(ref TreeNode treeNode)
	    {
	        //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
	        treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
	    }
	}
}