using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Trees;
using Umbraco.Web.UI.Controls;
using umbraco.interfaces;
using System.Text.RegularExpressions;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic.Utils;
using System.Text;
using umbraco.cms.presentation.Trees;
using umbraco.BasePages;
using System.Web.Services;
using System.Drawing;
using System.Linq;
using Umbraco.Core;

namespace umbraco.controls.Tree
{

    /// <summary>
    /// The Umbraco tree control.
    /// <remarks>If this control doesn't exist on an UmbracoEnsuredPage it will not work.</remarks>
    /// </summary>
    public partial class TreeControl : UmbracoUserControl, ITreeService
    {

        /// <summary>
        /// Set the defaults
        /// </summary>
        public TreeControl()
        {
            Width = Unit.Empty;
            Height = Unit.Empty;
            BackColor = Color.Empty;
            CssClass = "";
            ManualInitialization = false;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnableViewState = false;
        }

        public enum TreeMode
        {
            Standard, Checkbox, InheritedCheckBox
        }

        /// <summary>
        /// If there is not application or tree specified in a query string then this is the application to load.
        /// </summary>
        private const string DEFAULT_APP = Constants.Applications.Content;

        private List<BaseTree> m_ActiveTrees = new List<BaseTree>();
        private List<BaseTree> m_AllAppTrees = new List<BaseTree>();
        private List<ApplicationTree> m_ActiveTreeDefs = null;
        private TreeMode m_TreeType = TreeMode.Standard;
        private bool m_IsInit = false;
        private TreeService m_TreeService = new TreeService();
        private string m_SelectedNodePath;

        #region Public Properties

        #region Style Properties
        public string CssClass { get; set; }
        public Unit Height { get; set; }
        public Unit Width { get; set; }
        public Color BackColor { get; set; }
        #endregion

        #region TreeService parameters.
        public string FunctionToCall
        {
            get { return m_TreeService.FunctionToCall; }
            set
            {
                m_TreeService.FunctionToCall = value;
            }
        }

        public string NodeKey
        {
            get { return m_TreeService.NodeKey; }
            set
            {
                m_TreeService.NodeKey = value;
            }
        }

        public int StartNodeID
        {
            get { return m_TreeService.StartNodeID; }
            set
            {
                m_TreeService.StartNodeID = value;
            }
        }

        public string SelectedNodePath
        {
            get { return m_SelectedNodePath; }
            set
            {
                m_SelectedNodePath = value;
            }
        }

        public string TreeType
        {
            get { return m_TreeService.TreeType; }
            set
            {
                m_TreeService.TreeType = value;
            }
        }

        public bool ShowContextMenu
        {
            get { return m_TreeService.ShowContextMenu; }
            set
            {
                m_TreeService.ShowContextMenu = value;
            }
        }

        public bool IsDialog
        {
            get { return m_TreeService.IsDialog; }
            set
            {
                m_TreeService.IsDialog = value;
            }
        }

        public TreeDialogModes DialogMode
        {
            get { return m_TreeService.DialogMode; }
            set
            {
                m_TreeService.DialogMode = value;
            }
        }


        public string App
        {
            get
            {
                return GetCurrentApp();
            }
            set
            {
                m_TreeService.App = value;
            }
        }
        #endregion

        /// <summary>
        /// Allows for checkboxes to be used with the tree. Default is standard.
        /// </summary>
        public TreeMode Mode
        {
            get
            {
                return m_TreeType;
            }
            set
            {
                m_TreeType = value;
            }
        }

        /// <summary>
        /// Returns the required JavaScript as a string for the current application
        /// </summary>
        public string JSCurrApp
        {
            get
            {
                StringBuilder javascript = new StringBuilder();
                foreach (BaseTree bTree in m_AllAppTrees)
                    bTree.RenderJS(ref javascript);
                return javascript.ToString();
            }
        }

        /// <summary>
        /// By default this is false. If set to true, then the code in the client side of the tree will force calling rebuildTree
        /// to be called explicitly for the tree to render
        /// </summary>
        public bool ManualInitialization { get; set; }

        #endregion

        /// <summary>
        /// Can be set explicitly which will override what is in query strings or what has been set by properties.
        /// Useful for rendering out a tree dynamically with an instance of anoterh TreeService.
        /// By using this method, it will undo any of the tree service public properties that may be set
        /// on this object.
        /// </summary>
        public void SetTreeService(TreeService srv)
        {
            m_TreeService = srv;
            Initialize();
        }

        /// <summary>
        /// Initializes the control and looks up the tree structures that are required to be rendered.
        /// Properties of the control (or SetTreeService) need to be set before pre render or calling
        /// GetJSONContextMenu or GetJSONNode
        /// </summary>
        protected void Initialize()
        {
            //use the query strings if the TreeParams isn't explicitly set
            if (m_TreeService == null)
            {
                m_TreeService = TreeRequestParams.FromQueryStrings().CreateTreeService();
            }
            m_TreeService.App = GetCurrentApp();

            // Validate permissions
            if (!BasePages.BasePage.ValidateUserContextID(BasePages.BasePage.umbracoUserContextID))
                return;
            UmbracoEnsuredPage page = new UmbracoEnsuredPage();
            if (!page.ValidateUserApp(GetCurrentApp()))
                throw new ArgumentException("The current user doesn't have access to this application. Please contact the system administrator.");

            //find all tree definitions that have the current application alias that are ACTIVE.
            //if an explicit tree has been requested, then only load that tree in.
            //m_ActiveTreeDefs = TreeDefinitionCollection.Instance.FindActiveTrees(GetCurrentApp());

            m_ActiveTreeDefs = Services.ApplicationTreeService.GetApplicationTrees(GetCurrentApp(), true).ToList();
            
            if (!string.IsNullOrEmpty(this.TreeType))
            {
                m_ActiveTreeDefs = m_ActiveTreeDefs
                    .Where(x => x.Alias == this.TreeType)
                    .ToList(); //this will only return 1
            }

            //find all tree defs that exists for the current application regardless of if they are active
            var appTreeDefs = Services.ApplicationTreeService.GetApplicationTrees(GetCurrentApp()).ToList();

            //Create the BaseTree's based on the tree definitions found
            foreach (var treeDef in appTreeDefs)
            {
                //create the tree and initialize it
                var bTree = LegacyTreeDataConverter.GetLegacyTreeForLegacyServices(treeDef);
                //BaseTree bTree = treeDef.CreateInstance();
                bTree.SetTreeParameters(m_TreeService);

                //store the created tree
                m_AllAppTrees.Add(bTree);
                if (treeDef.Initialize)
                    m_ActiveTrees.Add(bTree);
            }

            m_IsInit = true;
        }

        /// <summary>
        /// This calls the databind method to bind the data binding syntax on the front-end.
        /// <remarks>
        /// Databinding was used instead of inline tags in case the tree properties needed to be set
        /// by other classes at runtime
        /// </remarks>
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// This will initialize the control so all TreeService properties need to be set before hand
        /// </remarks>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!m_IsInit)
                Initialize();

            //Render out the JavaScript associated with all of the trees for the application
            RenderTreeJS();

            //apply the styles
            if (Width != Unit.Empty)
                TreeContainer.Style.Add(HtmlTextWriterStyle.Width, Width.ToString());
            if (Height != Unit.Empty)
                TreeContainer.Style.Add(HtmlTextWriterStyle.Height, Height.ToString());
            if (BackColor != Color.Empty)
                TreeContainer.Style.Add(HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(BackColor));
            if (CssClass != "")
            {
                TreeContainer.Attributes.Add("class", CssClass);
            }
            else
            {
                //add the default class
                TreeContainer.Attributes.Add("class", "treeContainer");
            }


            DataBind();
        }

        /// <summary>
        /// Returns the JSON markup for the full context menu
        /// </summary>
        public string GetJSONContextMenu()
        {
            if (ShowContextMenu)
            {
                JTreeContextMenu menu = new JTreeContextMenu();
                return menu.RenderJSONMenu();
            }
            else
            {
                return "{}";
            }

        }

        /// <summary>
        /// Returns a string with javascript proxy methods for IActions that are using old javascript
        /// </summary>
        /// <returns></returns>
        public string GetLegacyIActionJavascript()
        {
            return LegacyTreeJavascript.GetLegacyIActionJavascript();
        }

        /// <summary>
        /// Returns the JSON markup for one node
        /// </summary>
        /// <param name="treeAlias"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        /// <remarks>
        /// This will initialize the control so all TreeService properties need to be set before hand
        /// </remarks>
        public string GetJSONNode(string nodeId)
        {
            if (!m_IsInit)
                Initialize();

            if (string.IsNullOrEmpty(m_TreeService.TreeType))
            {
                throw new ArgumentException("The TreeType is not set on the tree service");
            }

            BaseTree tree = m_ActiveTrees.Find(
                delegate(BaseTree t)
                {
                    return (t.TreeAlias == m_TreeService.TreeType);
                }
            );
            return tree.GetSerializedNodeData(nodeId);
        }

        /// <summary>
        /// Returns the JSON markup for the first node in the tree
        /// </summary>

        public string GetJSONInitNode()
        {
            if (!m_IsInit)
                Initialize();

            //if there is only one tree to render, we don't want to have a node to hold sub trees, we just want the
            //stand alone tree, so we'll just add a TreeType to the TreeService and ensure that the right method gets loaded in tree.aspx
            if (m_ActiveTrees.Count == 1)
            {
                m_TreeService.TreeType = m_ActiveTreeDefs[0].Alias;

                //convert the menu to a string
                //string initActions = (TreeSvc.ShowContextMenu ? Action.ToString(m_ActiveTrees[0].RootNodeActions) : "");

                //Since there's only 1 tree, render out the tree's RootNode properties
                XmlTree xTree = new XmlTree();
                xTree.Add(m_ActiveTrees[0].RootNode);
                return xTree.ToString();
            }
            else
            {

                //If there is more than 1 tree for the application than render out a 
                //container node labelled with the current application.
                XmlTree xTree = new XmlTree();
                XmlTreeNode xNode = XmlTreeNode.CreateRoot(new NullTree(GetCurrentApp()));
                xNode.Text = ui.Text("sections", GetCurrentApp(), UmbracoEnsuredPage.CurrentUser);
                xNode.Source = m_TreeService.GetServiceUrl();
                xNode.Action = "javascript:" + ClientTools.Scripts.OpenDashboard(GetCurrentApp());
                xNode.NodeType = m_TreeService.App.ToLower();
                xNode.NodeID = "-1";
                xNode.Icon = ".sprTreeFolder";
                xTree.Add(xNode);
                return xTree.ToString();
            }
        }

        private void RenderTreeJS()
        {
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "Trees_" + GetCurrentApp(), JSCurrApp, true);
        }

        /// <summary>
        /// Return the current application alias. If neither the TreeType of Application is specified
        /// than return the default application. If the Application is null but there is a TreeType then
        /// find the application that the tree type is associated with.
        /// </summary>
        private string GetCurrentApp()
        {
            //if theres an treetype specified but no application
            if (string.IsNullOrEmpty(m_TreeService.App) &&
                !string.IsNullOrEmpty(m_TreeService.TreeType))
            {
                TreeDefinition treeDef = TreeDefinitionCollection.Instance.FindTree(m_TreeService.TreeType);
                if (treeDef != null)
                    return treeDef.App.alias;
            }
            else if (!string.IsNullOrEmpty(m_TreeService.App))
                return m_TreeService.App;

            //if everything is null then return the default app
            return DEFAULT_APP;
        }

        /// <summary>
        /// CssInclude2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.CssInclude CssInclude2;

        /// <summary>
        /// CssInclude3 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.CssInclude CssInclude3;

        /// <summary>
        /// CssInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.CssInclude CssInclude1;

        /// <summary>
        /// JsInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

        /// <summary>
        /// JsInclude2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude2;

        /// <summary>
        /// JsInclude3 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude3;

        /// <summary>
        /// JsInclude4 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude4;

        /// <summary>
        /// JsInclude5 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude5;

        /// <summary>
        /// JsInclude6 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude6;

        /// <summary>
        /// JsInclude8 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude8;

        /// <summary>
        /// JsInclude11 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude11;

        /// <summary>
        /// JsInclude7 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude7;

        /// <summary>
        /// JsInclude12 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude12;

        /// <summary>
        /// JsInclude9 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude9;

        /// <summary>
        /// JsInclude10 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude10;

        /// <summary>
        /// TreeContainer control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl TreeContainer;
    }
}