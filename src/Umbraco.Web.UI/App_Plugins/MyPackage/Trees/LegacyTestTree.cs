using System.Text;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.UI.App_Plugins.MyPackage.Trees
{
    public class LegacyTestTree : BaseTree
    {
        public LegacyTestTree(string application) : base(application)
        {
        }

        public override string TreeAlias
        {
            get { return "legacyTestTree"; }
        }

        public override int StartNodeID
        {
            get { return -1; }
        }       

        public override void RenderJS(ref StringBuilder javascript)
        {            
        }

        public override void Render(ref XmlTree tree)
        {
            for (var i = 0; i < 10; i++)
            {
                var node = XmlTreeNode.Create(this);
                node.Text = "Node " + i;
                tree.Add(node);
            }
        }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            
        }
    }
}