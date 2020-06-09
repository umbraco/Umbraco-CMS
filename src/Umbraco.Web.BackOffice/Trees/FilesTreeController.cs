using Umbraco.Core;
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
        private readonly IIOHelper _ioHelper;
        private readonly ILogger _logger;

        protected override IFileSystem FileSystem { get; }

        private static readonly string[] ExtensionsStatic = { "*" };

        public FilesTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IIOHelper ioHelper,
            ILogger logger)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory)
        {
            _ioHelper = ioHelper;
            _logger = logger;
            FileSystem =  new PhysicalFileSystem(_ioHelper, _logger, "~/");
        }

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => Constants.Icons.MediaFile;
    }
}
