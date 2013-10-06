using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using Umbraco.Core.Services;
using umbraco.businesslogic;
using umbraco.interfaces;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;


namespace umbraco
{
    [Tree(Constants.Applications.Settings, "nodeTypes", "Document Types", sortOrder: 6)]
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
            var docTypes = Service.GetContentTypeChildren(base.m_id);

            foreach (var docType in docTypes)
            {
                var hasChildren = Service.HasChildren(docType.Id);

                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = docType.Id.ToString(CultureInfo.InvariantCulture);
                xNode.Text = GetTranslatedNodeTypeName(docType.Name);
                xNode.Action = "javascript:openNodeType(" + docType.Id + ");";
                xNode.Icon = "settingDataType.gif";
                xNode.OpenIcon = "settingDataType.gif";
                xNode.Source = GetTreeServiceUrl(docType.Id);
                xNode.HasChildren = hasChildren;
                if (hasChildren)
                {
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

        private IContentTypeService Service
        {
            get { return Services.ContentTypeService; }
        }

        private string GetTranslatedNodeTypeName(string originalName)
        {

            if (originalName.StartsWith("#") == false)
                return originalName;
            
            var lang = Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentCulture.Name);

            if (lang != null && Dictionary.DictionaryItem.hasKey(originalName.Substring(1, originalName.Length - 1)))
            {
                var dictionaryItem = new Dictionary.DictionaryItem(originalName.Substring(1, originalName.Length - 1));
                if (dictionaryItem != null)
                    return dictionaryItem.Value(lang.id);
            }

            return "[" + originalName + "]";
        }
    }
}
