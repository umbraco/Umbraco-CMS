using System;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    public abstract class FileSystemTreeController : TreeController
    {
        protected abstract string FilePath { get; }
        protected abstract string FileSearchPattern { get; }

        /// <summary>
        /// Inheritors can override this method to modify the file node that is created.
        /// </summary>
        /// <param name="treeNode"></param>
        protected virtual void OnRenderFileNode(ref TreeNode treeNode) { }

        /// <summary>
        /// Inheritors can override this method to modify the folder node that is created.
        /// </summary>
        /// <param name="treeNode"></param>
        protected virtual void OnRenderFolderNode(ref TreeNode treeNode) { }

        protected override TreeNodeCollection GetTreeNodes(string id, System.Net.Http.Formatting.FormDataCollection queryStrings)
        {
            var orgPath = "";
            string path;
            if (string.IsNullOrEmpty(id) == false && id != "-1")
            {
                orgPath = id;
                path = IOHelper.MapPath(FilePath + "/" + orgPath);
                orgPath += "/";
            }
            else
            {
                path = IOHelper.MapPath(FilePath);
            }

            var dirInfo = new DirectoryInfo(path);
            var dirInfos = dirInfo.GetDirectories();

            var nodes = new TreeNodeCollection();
            foreach (var dir in dirInfos)
            {
                if ((dir.Attributes & FileAttributes.Hidden) == 0)
                {
                    var hasChildren = dir.GetFiles().Length > 0 || dir.GetDirectories().Length > 0;
                    var node = CreateTreeNode(orgPath + dir.Name, orgPath, queryStrings, dir.Name, "icon-folder", hasChildren);

                    OnRenderFolderNode(ref node);
                    if (node != null)
                        nodes.Add(node);
                }
            }

            //this is a hack to enable file system tree to support multiple file extension look-up
            //so the pattern both support *.* *.xml and xml,js,vb for lookups
            var allowedExtensions = new string[0];
            var filterByMultipleExtensions = FileSearchPattern.Contains(",");
            FileInfo[] fileInfo;

            if (filterByMultipleExtensions)
            {
                fileInfo = dirInfo.GetFiles();
                allowedExtensions = FileSearchPattern.ToLower().Split(',');
            }
            else
                fileInfo = dirInfo.GetFiles(FileSearchPattern);

            foreach (var file in fileInfo)
            {
                if ((file.Attributes & FileAttributes.Hidden) == 0)
                {
                    if (filterByMultipleExtensions && Array.IndexOf(allowedExtensions, file.Extension.ToLower().Trim('.')) < 0)
                        continue;

                    var node = CreateTreeNode(orgPath + file.Name, orgPath, queryStrings, file.Name, "icon-file", false);

                    OnRenderFileNode(ref node);

                    if (node != null)
                        nodes.Add(node);
                }
            }
            return nodes;
        }
    }
}
