using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Umbraco.Core.Services;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Web.umbraco.presentation.umbraco.Trees;

namespace umbraco
{
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
    public class loadMediaTypes : BaseTree
    {
        public loadMediaTypes(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionNew.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionDelete.Instance);
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openMediaType(id) {
	UmbClientMgr.contentFrame('settings/editMediaType.aspx?id=' + id);
}
");
        }

        public override void Render(ref XmlTree tree)
        {
            var mediaTypes = Service.GetMediaTypeChildren(base.m_id);

            foreach (var mediaType in mediaTypes)
            {
                var hasChildren = Service.MediaTypeHasChildren(mediaType.Id);

                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = mediaType.Id.ToString(CultureInfo.InvariantCulture);
                xNode.Text = TranslateTreeNames.GetTranslatedName(mediaType.Name);
                xNode.Action = string.Format("javascript:openMediaType({0});", mediaType.Id);
                xNode.Icon = "icon-item-arrangement";
                xNode.OpenIcon = "icon-item-arrangement";
                xNode.Source = GetTreeServiceUrl(mediaType.Id);
                xNode.HasChildren = hasChildren;
          
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
    }
}
