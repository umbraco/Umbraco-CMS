using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, "files", TreeTitle = "Files", TreeUse = TreeUse.Dialog)]
    [CoreTree]
    public class FilesTreeController : FileSystemTreeController
    {
        private readonly IIOHelper _ioHelper;
        private readonly ILogger _logger;

        public FilesTreeController(IIOHelper ioHelper, ILogger logger)
        {
            _ioHelper = ioHelper;
            _logger = logger;
        }

        protected override IFileSystem FileSystem => new PhysicalFileSystem(_ioHelper, _logger, "~/");

        private static readonly string[] ExtensionsStatic = { "*" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => Constants.Icons.MediaFile;
    }
}
