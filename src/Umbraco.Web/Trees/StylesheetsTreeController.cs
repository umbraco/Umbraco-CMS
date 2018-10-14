﻿using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, Constants.Trees.Stylesheets, "Stylesheets", "icon-folder", "icon-folder", sortOrder: 9)]
    public class StylesheetsTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => Current.FileSystems.StylesheetsFileSystem; // fixme inject

        private static readonly string[] ExtensionsStatic = { "css" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-brackets";
    }
}
