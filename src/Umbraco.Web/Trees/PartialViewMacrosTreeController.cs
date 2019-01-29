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
    [Tree(Constants.Applications.Settings, Constants.Trees.PartialViewMacros, null, sortOrder: 8)]
    [UmbracoTreeAuthorize(Constants.Trees.PartialViewMacros)]
    [PluginController("UmbracoTrees")]
    [CoreTree(TreeGroup = Constants.Trees.Groups.Templating)]
    public class PartialViewMacrosTreeController : PartialViewsTreeController
    {
        protected override IFileSystem FileSystem => Current.FileSystems.MacroPartialsFileSystem;

        private static readonly string[] ExtensionsStatic = {"cshtml"};

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-article";
    }
}
