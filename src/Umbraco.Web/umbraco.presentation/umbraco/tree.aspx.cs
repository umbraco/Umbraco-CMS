using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;

using System.Reflection;
using umbraco.DataLayer;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;
using umbraco.BusinessLogic;

using System.Collections.Generic;

namespace umbraco.cms.presentation
{

    /// <summary>
    /// This still outputs the xml format of the tree in case developers are using it.
    /// </summary>
    [Obsolete("this is no longer used for the client side tree")]
    public partial class tree : umbraco.BasePages.UmbracoEnsuredPage
    {

        private XmlTree m_xTree = new XmlTree();
        TreeRequestParams m_treeParams = TreeRequestParams.FromQueryStrings();

        /// <summary>
        /// This checks to see which request parameters have been set for the Tree xml service
        /// to run. If there is no Tree Type specified, then this will return the xml structure
        /// of the initial tree nodes for all trees required for the current application. Otherwise
        /// this will return thre required tree xml based on the request parameters specified.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e)
        {

            if (string.IsNullOrEmpty(m_treeParams.TreeType))
                if (!string.IsNullOrEmpty(m_treeParams.Application))
                    LoadAppTrees(m_treeParams.Application);
                else
                    LoadNullTree();
            else
                LoadTree(m_treeParams.TreeType);

            Response.Write(m_xTree.ToString(SerializedTreeType.XmlTree));
        }

        /// <summary>
        /// If the application supports multiple trees, then this function iterates over all of the trees assigned to it
        /// and creates their top level nodes and context menus.
        /// </summary>
        /// <param name="appAlias"></param>
        private void LoadAppTrees(string appAlias)
        {
            //find all tree definitions that have the current application alias
            List<TreeDefinition> treeDefs = TreeDefinitionCollection.Instance.FindActiveTrees(appAlias);

            foreach (TreeDefinition treeDef in treeDefs)
            {
                BaseTree bTree = treeDef.CreateInstance();
                bTree.SetTreeParameters(m_treeParams);
                m_xTree.Add(bTree.RootNode);
            }
        }

        /// <summary>
        /// This will load the particular ITree object and call it's render method to get the nodes that need to be rendered.
        /// </summary>
        /// <param name="appAlias"></param>
        /// <param name="treeAlias"></param>
        private void LoadTree(string treeAlias)
        {

            TreeDefinition treeDef = TreeDefinitionCollection.Instance.FindTree(treeAlias);

            if (treeDef != null)
            {
                BaseTree bTree = treeDef.CreateInstance();
                bTree.SetTreeParameters(m_treeParams);
                bTree.Render(ref m_xTree);
            }
            else
                LoadNullTree();
        }

        /// <summary>
        /// Load an empty tree structure to show the end user that there was a problem loading the tree.
        /// </summary>
        private void LoadNullTree()
        {
            BaseTree nullTree = new NullTree(m_treeParams.Application);
            nullTree.SetTreeParameters(m_treeParams);
            nullTree.Render(ref m_xTree);
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion
    }


}
