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


namespace umbraco
{
	/// <summary>
	/// Handles loading of the cache application into the developer application tree
	/// </summary>
    [ApplicationTree("developer", "cacheBrowser", "Cache Browser")]
    public class loadCache : BaseTree
	{
        public loadCache(string application) : base(application) { }

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

        public override void RenderJS(ref StringBuilder Javascript) { }
        
        public override void Render(ref XmlTree tree)
        {
            Hashtable ht = Cache.ReturnCacheItemsOrdred();

            foreach (string key in ht.Keys)
            {
                //each child will need to load a CacheItem instead of a Cache tree so
                //we'll create a loadCacheItem object in order to get it's serivce url and alias properties
                loadCacheItem loadCacheItemTree = new loadCacheItem(this.app);
				int itemCount = ((ArrayList)ht[key]).Count;
                XmlTreeNode xNode = XmlTreeNode.Create(loadCacheItemTree);
                xNode.NodeID = key;
				xNode.Text = key + " (" + itemCount + ")";
				xNode.Action = string.Empty;
                xNode.Source = loadCacheItemTree.GetTreeServiceUrl(key);
                xNode.Icon = "developerCacheTypes.gif";
                xNode.OpenIcon = "developerCacheTypes.gif";
				xNode.HasChildren = itemCount > 0;

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
