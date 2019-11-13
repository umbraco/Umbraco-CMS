using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, "files", TreeTitle = "Files", TreeUse = TreeUse.Dialog)]
    [CoreTree]
    public class FilesTreeController : FileSystemTreeController
    {
        private readonly IIOHelper _ioHelper;
        private readonly IFileSystem _fileSystem;

        protected override IFileSystem FileSystem => _fileSystem;

        private static readonly string[] ExtensionsStatic = { "*" };

        public FilesTreeController(IIOHelper ioHelper)
        {
            _ioHelper = ioHelper;
            _fileSystem = new PhysicalFileSystem("~/", _ioHelper);
        }

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => Constants.Icons.MediaFile;
    }
}
