using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.IO;


namespace umbraco
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public class loadScripts : FileSystemTree
    {
        public loadScripts(string application) : base(application) { }
        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
            rootNode.Text = ui.Text("treeHeaders", "scripts");
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
			function openScriptEditor(id) {
			    UmbClientMgr.contentFrame('settings/scripts/editScript.aspx?file=' + id);
			}
            function openScriptFolder(id) {
			    return false;
			}
		");
        }

        protected override string FilePath
        {
            get
            {
                return SystemDirectories.Scripts + "/";
            }
        }

        protected override string FileSearchPattern
        {

            get { return UmbracoSettings.ScriptFileTypes; }
        }

        protected override void OnRenderFolderNode(ref XmlTreeNode xNode)
        {

            xNode.Menu = new List<IAction>(new IAction[]
            {
                ActionNew.Instance, 
                ContextMenuSeperator.Instance, 
                ActionDelete.Instance, 
                ContextMenuSeperator.Instance, 
                ActionRefresh.Instance
            });
            xNode.Action = "javascript:void(0)";
            xNode.NodeType = "scriptsFolder";
            xNode.Action = "javascript:void(0);";
        }

        protected override void OnRenderFileNode(ref XmlTreeNode xNode)
        {
            xNode.Action = xNode.Action.Replace("openFile", "openScriptEditor");

            // add special icons for javascript files
            if (xNode.Text.Contains(".js"))
            {
                xNode.Icon = "icon-script";
                xNode.OpenIcon = "icon-script";
            }
            else
            {
                xNode.Icon = "icon-code";
                xNode.OpenIcon = "icon-code";
            }

            xNode.Text = xNode.Text.StripFileExtension();
        }


    }

}
