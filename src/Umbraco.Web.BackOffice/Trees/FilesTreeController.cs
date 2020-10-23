using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.BackOffice.Trees
{
    [Tree(Constants.Applications.Settings, "files", TreeTitle = "Files", TreeUse = TreeUse.Dialog)]
    [CoreTree]
    public class FilesTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem { get; }

        private static readonly string[] ExtensionsStatic = { "*" };

        public FilesTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IFileSystem fileSystem)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory)
        {
            FileSystem = fileSystem;
        }

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => Constants.Icons.MediaFile;
    }
}
