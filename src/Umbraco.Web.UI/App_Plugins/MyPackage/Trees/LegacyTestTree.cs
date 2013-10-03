using System;
using System.Net.Http.Formatting;
using System.Text;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.Trees.Menu;
using umbraco.cms.presentation.Trees;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.UI.App_Plugins.MyPackage.Trees
{
    [Tree("settings", "myTree", "My Tree")]
    [PluginController("MyPackage")]
    public class MyCustomTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            //check if we're rendering the root node's children
            if (id == Constants.System.Root.ToInvariantString())
            {
                var tree = new TreeNodeCollection
                {
                    CreateTreeNode("1", queryStrings, "My Node 1"), 
                    CreateTreeNode("2", queryStrings, "My Node 2"), 
                    CreateTreeNode("3", queryStrings, "My Node 3")
                };
                return tree;
            }
            //this tree doesn't suport rendering more than 1 level
            throw new NotSupportedException();
        }
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();
            menu.Add(new MenuItem("create", "Create"));
            return menu;
        }
    }

    public class LegacyTestTree : BaseTree
    {
        public LegacyTestTree(string application)
            : base(application)
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