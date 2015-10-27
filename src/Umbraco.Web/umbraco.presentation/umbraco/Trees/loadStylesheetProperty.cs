using System;
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
using Umbraco.Core;


namespace umbraco
{
    [Tree(Constants.Applications.Settings, "stylesheetProperty", "Stylesheet Property", "", "", initialize: false)]
	public class loadStylesheetProperty : BaseTree
	{
        public loadStylesheetProperty(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {            
			rootNode.NodeType = "init" + TreeAlias;
			rootNode.NodeID = "init";
        }
         
		public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
			function openStylesheetProperty(name, prop) {
				UmbClientMgr.contentFrame('settings/stylesheet/property/editStylesheetProperty.aspx?id=' + name + '&prop=' + prop);
			}
			");
        }

        public override void Render(ref XmlTree tree)
        {
            var sheet = Services.FileService.GetStylesheetByName(NodeKey.EnsureEndsWith(".css"));
            
            foreach (var prop in sheet.Properties)
            {
                var sheetId = sheet.Path.TrimEnd(".css");
                var xNode = XmlTreeNode.Create(this);
                xNode.NodeID = sheetId + "_" + prop.Name;
                xNode.Text = prop.Name;
                xNode.Action = "javascript:openStylesheetProperty('" +
                    //Needs to be escaped for JS
                    HttpUtility.UrlEncode(sheet.Path) + 
                    "','" + prop.Name + "');";
                xNode.Icon = "icon-brackets";
                xNode.OpenIcon = "icon-brackets";

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
