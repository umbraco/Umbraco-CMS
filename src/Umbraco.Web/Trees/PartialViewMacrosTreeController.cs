using Umbraco.Core.IO;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Tree for displaying partial view macros in the developer app
    /// </summary>
    [Tree(Constants.Applications.Settings, Constants.Trees.PartialViewMacros, SortOrder = 8, TreeGroup = Constants.Trees.Groups.Templating)]
    [UmbracoTreeAuthorize(Constants.Trees.PartialViewMacros)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class PartialViewMacrosTreeController : PartialViewsTreeController
    {
        protected override IFileSystem FileSystem => Current.FileSystems.MacroPartialsFileSystem;

        private static readonly string[] ExtensionsStatic = {"cshtml"};

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-article";

        protected override void OnRenderFolderNode(ref TreeNode treeNode)
        {
            //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
            treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
        }
    }
}
