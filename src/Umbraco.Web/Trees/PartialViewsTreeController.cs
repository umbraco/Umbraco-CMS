using Umbraco.Core.IO;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Tree for displaying partial views in the settings app
    /// </summary>
    [Tree(Constants.Applications.Settings, Constants.Trees.PartialViews, SortOrder = 7, TreeGroup = Constants.Trees.Groups.Templating)]
    [UmbracoTreeAuthorize(Constants.Trees.PartialViews)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class PartialViewsTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => Current.FileSystems.PartialViewsFileSystem;

        private static readonly string[] ExtensionsStatic = {"cshtml"};

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-article";
    }
}
