using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, "files", TreeTitle = "Files", TreeUse = TreeUse.Dialog)]
    [CoreTree]
    public class FilesTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => new PhysicalFileSystem("~/");

        private static readonly string[] ExtensionsStatic = { "*" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-document";
    }
}
