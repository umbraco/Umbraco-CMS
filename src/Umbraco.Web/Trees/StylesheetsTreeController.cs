using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Trees
{
    [CoreTree]
    [Tree(Constants.Applications.Settings, Constants.Trees.Stylesheets, TreeTitle = "Stylesheets", SortOrder = 9, TreeGroup = Constants.Trees.Groups.Templating)]
    public class StylesheetsTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => Current.FileSystems.StylesheetsFileSystem; // TODO: inject

        private static readonly string[] ExtensionsStatic = { "css" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-brackets";
    }
}
