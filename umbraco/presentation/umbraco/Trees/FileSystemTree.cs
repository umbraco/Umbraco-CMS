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

namespace umbraco.cms.presentation.Trees
{
    public abstract class FileSystemTree : BaseTree
    {

        public FileSystemTree(string application) : base(application) { }
        
        public override abstract void RenderJS(ref System.Text.StringBuilder Javascript);
        protected override abstract void CreateRootNode(ref XmlTreeNode rootNode);

        protected abstract string FilePath { get;}
        protected abstract string FileSearchPattern { get;}

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
                path = HttpContext.Current.Server.MapPath(FilePath + orgPath);
                orgPath += "/";
            }
            else
            {
                path = HttpContext.Current.Server.MapPath(FilePath);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo[] dirInfos = dirInfo.GetDirectories();
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

                    tree.Add(xDirNode);
                }
            }
            FileInfo[] fileInfo = dirInfo.GetFiles(FileSearchPattern);
            foreach (FileInfo file in fileInfo)
            {
                if ((file.Attributes & FileAttributes.Hidden) == 0)
                {
                    XmlTreeNode xFileNode = XmlTreeNode.Create(this);
					xFileNode.NodeID = orgPath + file.Name;
                    xFileNode.Text = file.Name;
                    if (!((orgPath == "")))
                        xFileNode.Action = "javascript:openFile('" + orgPath  + file.Name + "');";
                    else
                        xFileNode.Action = "javascript:openFile('" + file.Name + "');";
                    xFileNode.Icon = "doc.gif";
                    xFileNode.OpenIcon = "doc.gif";

                    OnRenderFileNode(ref xFileNode);

                    tree.Add(xFileNode);
                }
            }
        }

        
        
    }
}
