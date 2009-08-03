using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;

using umbraco.cms.presentation.Trees;

namespace umbraco.presentation.controls {
    public class CheckboxTree : System.Web.UI.WebControls.TreeView {

       protected override void OnInit(EventArgs e) {
            base.OnInit(e);
            if (ScriptManager.GetCurrent(this.Page) == null)
                throw new Exception("This control requires a ScriptManager");

            //RenderJS();
            RenderJS();
            
        }

        protected override void Render(HtmlTextWriter writer) {
            base.Render(writer);
        }
        /// <summary>
        /// Return the instance on the loadContent class. This lazily instantiates the loadContent object and if it is
        /// already created, it will return the already created instance.
        /// </summary>
        private loadContent ContentTree {
            get {
                if (m_contentTree != null)
                    return m_contentTree;

                m_contentTree = new loadContent("content");
                m_contentTree.FunctionToCall = m_clientNodeClicked;
                m_contentTree.IsDialog = true;
                m_contentTree.DialogMode = TreeDialogModes.id;
                return m_contentTree;
            }
        }

        private loadContent m_contentTree;
        private string m_clientNodeChecked = "void(0);";
        private string m_clientNodeClicked = "void(0);";
        private bool m_showCheckBoxes = true;
        private string m_imagesRootPath = GlobalSettings.Path + "/images/umbraco/";
        private string m_unpublishedColor = "#999999";

        /// <summary>
        /// The JavaScript method to call when a node is clicked. The method will receive a node ID as it's parameter.
        /// </summary>
        public string OnClientNodeClicked {
            get { return m_clientNodeClicked; }
            set { m_clientNodeClicked = value; }
        }

        /// <summary>
        /// The JavaScript method to call when a node is checked. The method will receive a comma seperated list of node IDs that are checked.
        /// </summary>
        public string OnClientNodeChecked {
            get { return m_clientNodeChecked; }
            set { m_clientNodeChecked = value; }
        }

        public bool ShowCheckBoxes {
            get { return m_showCheckBoxes; }
            set { m_showCheckBoxes = value; }
        }

        public string ImagesRootPath {
            get { return m_imagesRootPath; }
            set { m_imagesRootPath = value; }
        }

        public string UnpublishedColor {
            get { return m_unpublishedColor; }
            set { m_unpublishedColor = value; }
        }

        /// <summary>
        /// The JavaScript function that is called when a node is checked
        /// </summary>
        protected const string JSSelectNode = "ContentTreeControl_CheckedNode";

        /// <summary>
        /// Registers JavaScript for the contentTree
        /// </summary>
        private void RenderJS() {
            StringBuilder sb = new StringBuilder();
            ContentTree.RenderJS(ref sb);
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "treeJS", sb.ToString(), true);
        }

        /// <summary>
        /// Get the XmlTree for the Content Tree for the node ID passed in.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public static XmlTree GetNodeData(int? nodeID, CheckboxTree cbTree) {
            XmlTree xTree = new XmlTree();
            cbTree.ContentTree.id = (nodeID == null ? cbTree.ContentTree.StartNodeID : nodeID.Value);
            cbTree.ContentTree.Render(ref xTree);
            return xTree;
        }

        /// <summary>
        /// Converts an XmlTree to a TreeNodeCollection 
        /// </summary>
        /// <param name="xTree"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        public static TreeNodeCollection FromXmlTree(XmlTree xTree, TreeNode parentNode, CheckboxTree cbTree) {
            TreeNodeCollection nodes = new TreeNodeCollection(parentNode);
            foreach (XmlTreeNode xNode in xTree) {
                if (xNode != null) {
                    TreeNode node = FromXmlTreeNode(xNode, cbTree);
                    nodes.Add(node);
                }
            }
            return nodes;
        }

        /// <summary>
        /// Converts an XmlTreeNode to a TreeNode
        /// </summary>
        /// <param name="xNode"></param>
        /// <returns></returns>
        public static TreeNode FromXmlTreeNode(XmlTreeNode xNode, CheckboxTree cbTree) {
            TreeNode node = new TreeNode(xNode.Text, xNode.NodeID, cbTree.ImagesRootPath + xNode.Icon, xNode.Action, "");
            node.ImageUrl = "";

            node.PopulateOnDemand = (string.IsNullOrEmpty(xNode.Source) ? false : true);
            //node.SelectAction = (string.IsNullOrEmpty(xNode.Source) ? TreeNodeSelectAction.Select : TreeNodeSelectAction.Expand);
            node.SelectAction = TreeNodeSelectAction.None;

            string nodeMarkup = "";

            //add our custom checkbox
            if (cbTree.ShowCheckBoxes)
                nodeMarkup += string.Format("<span class=\"treeCheckBox\"><input type='checkbox' id='{0}' name='{0}' value='{1}' onclick='{2}(this);' /></span>",
                    "chk" + node.Value, node.Value, cbTree.OnClientNodeChecked);

            if (xNode.Icon.StartsWith(".")) {
                nodeMarkup += "<div class=\"sprTree " + xNode.Icon.TrimStart('.') + "\"><img class=\"webfx-tree-icon\" src=\"../images/nada.gif\"/> </div>";
            } else {
                nodeMarkup += "<span class=\"treeIcon\"><img src=\"" + cbTree.ImagesRootPath + xNode.Icon + "\"/></span>";
            }

            //show the node as being not published
            if (xNode.NotPublished.Value)
                nodeMarkup += string.Format("<span class=\"treeText\" style='color:{0};'>{1}</span>", cbTree.UnpublishedColor, node.Text);
            else {
                nodeMarkup += string.Format("<span class=\"treeText\">{0}</span>", node.Text);
            }

            node.Text = nodeMarkup;
            
            return node;
        }

        /*
        /// <summary>
        /// Populates child nodes on demand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void objTree_TreeNodePopulate(object sender, TreeNodeEventArgs e) {
            XmlTree xTree = GetNodeData(int.Parse(e.Node.Value));
            TreeNodeCollection nodes = FromXmlTree(xTree, e.Node);
            foreach (TreeNode node in nodes)
                e.Node.ChildNodes.Add(node);
        }  */  
    }

}
