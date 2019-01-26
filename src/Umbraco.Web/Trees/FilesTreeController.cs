using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [CoreTree(TreeGroup = Constants.Trees.Groups.Templating)]
    [Tree(Constants.Applications.Settings, "files", "Files", "icon-folder", "icon-folder", sortOrder: 13, initialize: false)]
    public class FilesTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => new PhysicalFileSystem("~/"); // todo inject

        private static readonly string[] ExtensionsStatic = { "*" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-document";
    }
}
