﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
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
using Umbraco.Core;


namespace umbraco
{
    [Tree(Constants.Applications.Settings, "stylesheets", "Stylesheets")]
	public class loadStylesheets : BaseTree
	{
        public loadStylesheets(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {                        
			rootNode.NodeType = "init" + TreeAlias;
			rootNode.NodeID = "init";
            rootNode.Text = ui.Text("treeHeaders", "stylesheets");
        }

		public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
			function openStylesheet(id) {
				UmbClientMgr.contentFrame('settings/stylesheet/editStylesheet.aspx?id=' + id);
			}
			");
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.AddRange(new IAction[] { ActionNew.Instance, ActionDelete.Instance, 
                ActionSort.Instance, ContextMenuSeperator.Instance, ActionRefresh.Instance });
        }

        public override void Render(ref XmlTree tree)
        {            
            foreach (StyleSheet n in StyleSheet.GetAll())
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = n.Id.ToString(CultureInfo.InvariantCulture);
                xNode.Text = n.Text;
                xNode.Action = "javascript:openStylesheet(" + n.Id + ");";
                loadStylesheetProperty styleSheetPropertyTree = new loadStylesheetProperty(this.app);
                xNode.Source = styleSheetPropertyTree.GetTreeServiceUrl(n.Id);
				xNode.HasChildren = n.HasChildren;
                xNode.Icon = " icon-brackets";
                xNode.OpenIcon = "icon-brackets";
                xNode.NodeType = "stylesheet"; //this shouldn't be like this, it should be this.TreeAlias but the ui.config file points to this name.

                OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                if (xNode != null)
                {
                    tree.Add(xNode);
                    OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                }
                
            }
        }

	}
    
}
