using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.BackOffice.Trees
{
    [CoreTree]
    [Tree(Constants.Applications.Settings, Constants.Trees.Stylesheets, TreeTitle = "Stylesheets", SortOrder = 9, TreeGroup = Constants.Trees.Groups.Templating)]
    public class StylesheetsTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem { get; }

        private static readonly string[] ExtensionsStatic = { "css" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-brackets";

        public StylesheetsTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IFileSystems fileSystems)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory)
        {
            FileSystem = fileSystems.StylesheetsFileSystem;
        }
    }
}
