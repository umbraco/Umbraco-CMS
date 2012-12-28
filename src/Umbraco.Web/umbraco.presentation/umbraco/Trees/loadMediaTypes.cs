using System;
using System.Text;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.presentation.Trees;

namespace umbraco
{
    [Tree("settings", "mediaTypes", "Media Types", sortOrder: 5)]
    public class loadMediaTypes : BaseTree
    {
        public loadMediaTypes(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
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
            foreach (var dt in MediaType.GetAllAsList())
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = dt.Id.ToString();
                xNode.Text = dt.Text;
                xNode.Action = string.Format("javascript:openMediaType({0});", dt.Id);
                xNode.Icon = "settingDataType.gif";
                xNode.OpenIcon = "settingDataType.gif";

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
