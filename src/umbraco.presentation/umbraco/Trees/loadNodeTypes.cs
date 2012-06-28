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
using umbraco.BusinessLogic.Actions;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;


namespace umbraco
{
    [ApplicationTree("settings", "nodeTypes", "Document Types", sortOrder: 6)]
    public class loadNodeTypes : BaseTree
    {

        public loadNodeTypes(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
			rootNode.NodeType = "init" + TreeAlias;
			rootNode.NodeID = "init";
        }

        protected override void CreateRootNodeActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionNew.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionImport.Instance);
            actions.Add(ActionCodeExport.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionRefresh.Instance);
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionNew.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionCopy.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionExport.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionDelete.Instance);
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openNodeType(id) {
	UmbClientMgr.contentFrame('settings/editNodeTypeNew.aspx?id=' + id);
}
");
        }

        public override void Render(ref XmlTree tree)
        {
            List<DocumentType> docTypes;
            if (base.m_id == -1)
                docTypes = DocumentType.GetAllAsList().FindAll(delegate(DocumentType dt) { return dt.MasterContentType == 0; });
            else
                docTypes = DocumentType.GetAllAsList().FindAll(delegate(DocumentType dt) { return dt.MasterContentType == base.m_id; });

            foreach (DocumentType dt in docTypes)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = dt.Id.ToString();
                xNode.Text = dt.Text;
                xNode.Action = "javascript:openNodeType(" + dt.Id + ");";
                xNode.Icon = "settingDataType.gif";
                xNode.OpenIcon = "settingDataType.gif";
				xNode.Source = GetTreeServiceUrl(dt.Id);
				xNode.HasChildren = dt.HasChildren;
                if (dt.HasChildren) {                    
                    xNode.Icon = "settingMasterDataType.gif";
                    xNode.OpenIcon = "settingMasterDataType.gif";
                }

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
