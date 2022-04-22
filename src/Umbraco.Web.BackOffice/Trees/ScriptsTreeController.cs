using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [CoreTree]
    [Tree(Constants.Applications.Settings, Constants.Trees.Scripts, TreeTitle = "Scripts", SortOrder = 10, TreeGroup = Constants.Trees.Groups.Templating)]
    public class ScriptsTreeController : FileSystemTreeController
    {
        protected override IFileSystem? FileSystem { get; }

        private static readonly string[] ExtensionsStatic = { "js" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-script";

        public ScriptsTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            FileSystems fileSystems,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator)
        {
            FileSystem = fileSystems.ScriptsFileSystem;
        }
    }
}
