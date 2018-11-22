using System;
using System.ComponentModel;
using System.Text;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.IO;
using Umbraco.Core;


namespace umbraco
{
    [Obsolete("This is no longer used and will be removed in future versions")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class loadDLRScripts : FileSystemTree
    {
        public loadDLRScripts(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
            rootNode.Text = ui.Text("treeHeaders", "scripting");
        }

        /// <summary>
        /// Renders the Javascript.
        /// </summary>
        /// <param name="Javascript">The javascript.</param>
        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(@"function openDLRScript(id) {UmbClientMgr.contentFrame('developer/python/editPython.aspx?file=' + id);}");
        }

        protected override string FilePath
        {
            get
            {
                return SystemDirectories.MacroScripts + "/";
            }
        }

        protected override string FileSearchPattern
        {
            get
            {
                return "*.*";
            }
        }

        protected override void OnRenderFileNode(ref XmlTreeNode xNode)
        {

            xNode.Action = xNode.Action.Replace("openFile", "openDLRScript");
            string ex = xNode.Text.Substring(xNode.Text.LastIndexOf('.')).Trim('.').ToLower();
            string icon = "developerScript.gif";

            switch (ex)
            {
                case "rb":
                    icon = "developerRuby.gif";
                    break;
                case "py":
                    icon = "developerPython.gif";
                    break;
                case "config":
                    //remove all config files
                    xNode = null;
                    return;
                default:
                    icon = "developerScript.gif";
                    break;
            }

            xNode.Icon = icon;
            xNode.OpenIcon = icon;

            xNode.Text = xNode.Text.StripFileExtension();
        }

    }

}
