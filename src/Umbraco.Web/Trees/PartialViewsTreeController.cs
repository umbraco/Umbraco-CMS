﻿using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Tree for displaying partial views in the settings app
    /// </summary>
    [Tree(Constants.Applications.Settings, "partialViews", "Partial Views", sortOrder: 2)]
	public class PartialViewsTreeController : FileSystemTreeController
    {
		protected override IFileSystem2 FileSystem
		{
			get { return FileSystemProviderManager.Current.PartialViewsFileSystem; }
		}

        private static readonly string[] ExtensionsStatic = { "cshtml" };

        protected override string[] Extensions
        {
            get { return ExtensionsStatic; }
        }

        protected override string FileIcon
        {
            get { return "icon-article"; }
        }

        protected override void OnRenderFolderNode(ref TreeNode treeNode)
        {
            //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
            treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
        }
    }
}
