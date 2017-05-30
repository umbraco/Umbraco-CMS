using System.Collections.Generic;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, Constants.Trees.Xslt, "XSLT Files", "icon-folder", "icon-folder", sortOrder: 2)]
    public class XsltTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => Current.FileSystems.XsltFileSystem; // fixme inject

        private static readonly string[] ExtensionsStatic = { "xslt" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-code";

        protected override bool EnableCreateOnFolder => true;
    }
}
