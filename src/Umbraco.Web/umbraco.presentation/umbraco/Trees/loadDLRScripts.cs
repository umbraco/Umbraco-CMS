using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
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
using Umbraco.Core.IO;


namespace umbraco
{
    /// <summary>
    /// Handles loading of python items into the developer application tree
    /// </summary>
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
        }

    }

}
