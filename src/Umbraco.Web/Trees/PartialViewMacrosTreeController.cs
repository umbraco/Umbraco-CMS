using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.BusinessLogic.Actions;
using Umbraco.Web.Models.Trees;
using System.Net.Http.Formatting;
using Umbraco.Core.Services;

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

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {

                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);

                return menu;
            }

            // TODO: Wire up new delete dialog
            menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
            return menu;
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
