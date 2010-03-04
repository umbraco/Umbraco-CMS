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
using umbraco.cms.businesslogic.template;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;

namespace umbraco
{
    public class loadTemplates : BaseTree
    {
        public loadTemplates(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {                        
			rootNode.NodeType = "init" + TreeAlias;
			rootNode.NodeID = "init";
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openTemplate(id) {
	parent.right.document.location.href = 'settings/editTemplate.aspx?templateID=' + id;
}
");
        }

        public override void Render(ref XmlTree tree)
        {
            List<Template> templates = null;
            if (base.m_id == -1)
                templates = Template.GetAllAsList().FindAll(delegate(Template t) { return !t.HasMasterTemplate; });
            else
                templates = Template.GetAllAsList().FindAll(delegate(Template t) { return t.MasterTemplate == base.m_id; });

            foreach (Template t in templates)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = t.Id.ToString();
                xNode.Text = t.Text;
                xNode.Action = "javascript:openTemplate(" + t.Id + ");";
                xNode.Icon = "settingTemplate.gif";
                xNode.OpenIcon = "settingTemplate.gif";
				xNode.Source = GetTreeServiceUrl(t.Id);
				xNode.HasChildren = t.HasChildren;
                if (t.HasChildren) {                    
                    xNode.Icon = "settingMasterTemplate.gif";
                    xNode.OpenIcon = "settingMasterTemplate.gif";
                }

                OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                if (xNode != null)
                {
                    tree.Add(xNode);
                    OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                }
                
            }
        }

        protected override void CreateAllowedActions(ref List<IAction> actions) {
            actions.Clear();
            actions.AddRange(new IAction[] { ActionNew.Instance, ActionDelete.Instance, 
                ContextMenuSeperator.Instance, ActionRefresh.Instance });
        }
    }
    
}
