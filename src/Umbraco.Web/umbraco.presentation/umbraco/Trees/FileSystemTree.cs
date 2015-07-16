using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using Umbraco.Core.IO;

namespace umbraco.cms.presentation.Trees
{
    public abstract class FileSystemTree : BaseTree
    {

        public FileSystemTree(string application) : base(application) { }

        public override abstract void RenderJS(ref System.Text.StringBuilder Javascript);
        protected override abstract void CreateRootNode(ref XmlTreeNode rootNode);

        protected abstract string FilePath { get; }
        protected abstract string FileSearchPattern { get; }

        /// <summary>
        /// Inheritors can override this method to modify the file node that is created.
        /// </summary>
        /// <param name="xNode"></param>
        protected virtual void OnRenderFileNode(ref XmlTreeNode xNode) { }

        /// <summary>
        /// Inheritors can override this method to modify the folder node that is created.
        /// </summary>
        /// <param name="xNode"></param>
        protected virtual void OnRenderFolderNode(ref XmlTreeNode xNode) { }

        public override void Render(ref XmlTree tree)
        {
            string orgPath = "";
            string path = "";
            if (!string.IsNullOrEmpty(this.NodeKey))
            {
                orgPath = this.NodeKey;
                path = IOHelper.MapPath(FilePath + orgPath);
                orgPath += "/";
            }
            else
            {
                path = IOHelper.MapPath(FilePath);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(path);


            DirectoryInfo[] dirInfos = dirInfo.Exists ? dirInfo.GetDirectories() : new DirectoryInfo[] { };

            var args = new TreeEventArgs(tree);
            OnBeforeTreeRender(dirInfo, args);

            foreach (DirectoryInfo dir in dirInfos)
            {
                if ((dir.Attributes & FileAttributes.Hidden) == 0)
                {
                    XmlTreeNode xDirNode = XmlTreeNode.Create(this);
                    xDirNode.NodeID = orgPath + dir.Name;
                    xDirNode.Menu.Clear();
                    xDirNode.Text = dir.Name;
                    xDirNode.Action = string.Empty;
                    xDirNode.Source = GetTreeServiceUrl(orgPath + dir.Name);
                    xDirNode.Icon = FolderIcon;
                    xDirNode.OpenIcon = FolderIconOpen;
                    xDirNode.HasChildren = dir.GetFiles().Length > 0 || dir.GetDirectories().Length > 0;

                    OnRenderFolderNode(ref xDirNode);
                    OnBeforeNodeRender(ref tree, ref xDirNode, EventArgs.Empty);
                    if (xDirNode != null)
                    {
                        tree.Add(xDirNode);
                        OnAfterNodeRender(ref tree, ref xDirNode, EventArgs.Empty);
                    }


                }
            }

            //this is a hack to enable file system tree to support multiple file extension look-up
            //so the pattern both support *.* *.xml and xml,js,vb for lookups
            string[] allowedExtensions = new string[0];
            bool filterByMultipleExtensions = FileSearchPattern.Contains(",");
            FileInfo[] fileInfo;

            if (filterByMultipleExtensions)
            {
                fileInfo = dirInfo.Exists ? dirInfo.GetFiles() : new FileInfo[] {};
                allowedExtensions = FileSearchPattern.ToLower().Split(',');
            }
            else
            {
                fileInfo = dirInfo.Exists ? dirInfo.GetFiles(FileSearchPattern) : new FileInfo[] { };
            }

            foreach (FileInfo file in fileInfo)
            {
                if ((file.Attributes & FileAttributes.Hidden) == 0)
                {
                    if (filterByMultipleExtensions && Array.IndexOf<string>(allowedExtensions, file.Extension.ToLower().Trim('.')) < 0)
                        continue;

                    XmlTreeNode xFileNode = XmlTreeNode.Create(this);
                    xFileNode.NodeID = orgPath + file.Name;
                    xFileNode.Text = file.Name;
                    if (!((orgPath == "")))
                        xFileNode.Action = "javascript:openFile('" + orgPath + file.Name + "');";
                    else
                        xFileNode.Action = "javascript:openFile('" + file.Name + "');";
                    xFileNode.Icon = "doc.gif";
                    xFileNode.OpenIcon = "doc.gif";

                    OnRenderFileNode(ref xFileNode);
                    OnBeforeNodeRender(ref tree, ref xFileNode, EventArgs.Empty);
                    if (xFileNode != null)
                    {
                        tree.Add(xFileNode);
                        OnAfterNodeRender(ref tree, ref xFileNode, EventArgs.Empty);
                    }


                }
            }
            OnAfterTreeRender(dirInfo, args);
        }
    }
}
