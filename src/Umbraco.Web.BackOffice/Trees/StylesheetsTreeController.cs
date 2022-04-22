using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [CoreTree]
    [Tree(Constants.Applications.Settings, Constants.Trees.Stylesheets, TreeTitle = "Stylesheets", SortOrder = 9, TreeGroup = Constants.Trees.Groups.Templating)]
    public class StylesheetsTreeController : FileSystemTreeController
    {
        protected override IFileSystem? FileSystem { get; }

        private static readonly string[] ExtensionsStatic = { "css" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-brackets";

        public StylesheetsTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            FileSystems fileSystems,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator)
        {
            FileSystem = fileSystems.StylesheetsFileSystem;
        }
    }
}
