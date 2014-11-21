using System;
using System.Text;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.web;
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
			function openStylesheetProperty(id) {
				UmbClientMgr.contentFrame('settings/stylesheet/property/editStylesheetProperty.aspx?id=' + id);
			}
			");
        }

        public override void Render(ref XmlTree tree)
        {
            StyleSheet sn = new StyleSheet(m_id);
            
            foreach (StylesheetProperty n in sn.Properties)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = n.Id.ToString();
                xNode.Text = n.Text;
                xNode.Action = "javascript:openStylesheetProperty(" + n.Id + ");";
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
