using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Trees
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
            IHostingEnvironment hostingEnvironment,
            IIOHelper ioHelper,
            ILogger logger)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory)
           {
               FileSystem = new PhysicalFileSystem(ioHelper, hostingEnvironment, logger, "~/");
        }

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => Constants.Icons.MediaFile;
    }
}
