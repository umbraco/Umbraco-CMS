using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [CoreTree]
    [Tree(Constants.Applications.Settings, Constants.Trees.Scripts, TreeTitle = "Scripts", SortOrder = 10, TreeGroup = Constants.Trees.Groups.Templating)]
    public class ScriptsTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => Current.FileSystems.ScriptsFileSystem; // TODO: inject

        private static readonly string[] ExtensionsStatic = { "js" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-script";
    }
}
