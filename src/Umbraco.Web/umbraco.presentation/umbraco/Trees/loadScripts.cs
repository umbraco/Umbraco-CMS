using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using Umbraco.Core;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.contentitem;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core.IO;
using Umbraco.Core;


namespace umbraco
{
    [Tree(Constants.Applications.Settings, "scripts", "Scripts", "icon-folder", "icon-folder", sortOrder: 2)]
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
            
            xNode.Menu = new List<IAction>(new IAction[] { ActionDelete.Instance, ContextMenuSeperator.Instance, ActionNew.Instance, ContextMenuSeperator.Instance, ActionRefresh.Instance });
            xNode.NodeType = "scriptsFolder";
        }

        protected override void OnRenderFileNode(ref XmlTreeNode xNode)
        {
            xNode.Action = xNode.Action.Replace("openFile", "openScriptEditor");
            xNode.Icon = "icon-code";
            xNode.OpenIcon = "icon-code";
        }

        
    }
    
}
