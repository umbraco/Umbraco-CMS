using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [CoreTree(TreeGroup = Constants.Trees.Groups.Templating)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Scripts, "Scripts", "icon-folder", "icon-folder", sortOrder: 10)]
    public class ScriptsTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => Current.FileSystems.ScriptsFileSystem; // fixme inject

        private static readonly string[] ExtensionsStatic = { "js" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-script";
    }
}
