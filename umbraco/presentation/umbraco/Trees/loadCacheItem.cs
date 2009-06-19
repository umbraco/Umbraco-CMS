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
using umbraco.BusinessLogic.Actions;


namespace umbraco
{
	/// <summary>
	/// Handles loading of each individual cache items into the application tree under the cache application 
	/// </summary>
	public class loadCacheItem : BaseTree
	{
        public loadCacheItem(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {   
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

        protected override void CreateRootNodeActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionRefresh.Instance);
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionRefresh.Instance);
        }

		/// <summary>
		/// Renders the javascript.
		/// </summary>
		/// <param name="Javascript">The javascript.</param>
		public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openCacheItem(id) {
	parent.right.document.location.href = 'developer/cache/viewCacheItem.aspx?key=' + id;
}
");
        }

        public override void Render(ref XmlTree tree)
        {            
            Hashtable ht = Cache.ReturnCacheItemsOrdred();

            ArrayList a = (ArrayList)ht[this.NodeKey];

            for (int i = 0; i < a.Count; i++)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = a[i].ToString();
                xNode.Text = a[i].ToString();
                xNode.Action = "javascript:openCacheItem('" + a[i] + "');";
                xNode.Icon = "developerCacheItem.gif";
                xNode.OpenIcon = "developerCacheItem.gif";
                
                tree.Add(xNode);
            }
        }
		
	}
    
}
