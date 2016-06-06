﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;

namespace umbraco.cms.presentation.Trees
{
    /// <summary>
    /// All ITree's should inherit from BaseTree.
    /// </summary>
    public abstract class BaseTree : ITree, ITreeService
    {

        public BaseTree(string application)
        {
            this.app = application;
        }

        protected const string FolderIcon = ".sprTreeFolder";
        protected const string FolderIconOpen = ".sprTreeFolder_o";        

        /// <summary>
        /// Returns the node definition of the root node for this tree
        /// </summary>
        public XmlTreeNode RootNode
        {
            get 
            {
                Initialize();
                return m_initNode; 
            }
        }

        /// <summary>
        /// By default the init actions that are allowed for all trees are Create, Reload Nodes.
        /// These are the menu items that show up in the context menu for the root node of the current tree.
        /// Should be used in conjunction with the RootNode property
        /// </summary>
        public List<IAction> RootNodeActions
        {
            get
            {
                Initialize();
                return m_initActions;
            }
        }

        /// <summary>
        /// The actions that are allowed to be performed on this tree. These are the items that may show up on the
        /// context menu for a given node.
        /// </summary>
        public List<IAction> AllowedActions
        {
            get
            {
                Initialize();
                return m_allowedActions;
            }
        }

        /// <summary>
        /// The tree alias name. By default, if a BaseTree is instantiated by it's TreeDefinition, then the TreeAlias will be
        /// the name defined in the database. Inheritors can override this property to set the TreeAlias to whatever they choose.
        /// </summary>
        public virtual string TreeAlias
        {
            get
            {
                if (string.IsNullOrEmpty(m_treeAlias))
                {
                    TreeDefinition treeDef = TreeDefinitionCollection.Instance.FindTree(this);
                    m_treeAlias = (treeDef != null ? treeDef.Tree.Alias : "");
                }

                return m_treeAlias;
            }
        }
        private string m_treeAlias;

        #region ITreeService Members

        /// <summary>
        /// By default the start node id will be -1 which will return all of the nodes
        /// </summary>
        public virtual int StartNodeID
        {
            get { return -1; }
        }

        public bool ShowContextMenu
        {
            get { return m_showContextMenu; }
            set { m_showContextMenu = value; }
        }

        public bool IsDialog
        {
            get { return m_isDialog; }
            set { m_isDialog = value; }
        }

        /// <summary>
        /// The NodeKey is a string representation of the nodeID. Generally this is used for tree's whos node's unique key value is a string in instead 
        /// of an integer such as folder names.
        /// </summary>
        public string NodeKey
        {
            get { return m_nodeKey; }
            set { m_nodeKey = value; }
        }

        public string FunctionToCall
        {
            get { return m_functionToCall; }
            set { m_functionToCall = value; }
        }

        public TreeDialogModes DialogMode
        {
            get { return m_dialogMode; }
            set { m_dialogMode = value; }
        }

        #endregion

        #region ITree Members

        /// <summary>
        /// The ID of the node to render. This is generally set before calling the render method of the tree. If it is not set then the 
        /// StartNodeID property is used as the node ID to render.
        /// </summary>
        public virtual int id 
        { 
            set { m_id = value; }
            get { return m_id; }
        }
        public virtual string app 
        { 
            set { m_app = value; }
            get { return m_app; }
        }

        /// <summary>
        /// Renders out any JavaScript methods that may be required for tree functionality. Generally used to load the editor page when
        /// a user clicks on a tree node.
        /// </summary>
        /// <param name="Javascript"></param>
        public abstract void RenderJS(ref StringBuilder Javascript);

        /// <summary>
        /// This will call the new Render method which works using a typed XmlTree object instead of the untyped XmlDocument object.
        /// This can still be overriden but is only for backwards compatibility.
        /// </summary>
        /// <param name="Tree"></param>
        [Obsolete("Use the other Render method instead")]
        public virtual void Render(ref XmlDocument Tree)
        {
            //call our render method by passing in the XmlTree instead of the XmlDocument
            Render(ref m_xTree);
            //now that we have an XmlTree object filled, we'll serialize it back to the XmlDocument of the ITree
			Tree.LoadXml(m_xTree.ToString(SerializedTreeType.XmlTree));			
        }

        /// <summary>
        /// Classes need to override thid method to create the nodes for the XmlTree
        /// </summary>
        /// <param name="tree"></param>
        public abstract void Render(ref XmlTree tree);

        #endregion

        protected int m_id;
        protected string m_app;
        protected XmlTreeNode m_initNode;
        private List<IAction> m_initActions = new List<IAction>();
        private List<IAction> m_allowedActions = new List<IAction>();
        
        //these are the request parameters that can be specified.
        //since we want to remove the querystring/httpcontext dependency from
        //our trees, we need to define these as properties.       
        private bool m_showContextMenu = true;
        private bool m_isDialog = false;
        private TreeDialogModes m_dialogMode = TreeDialogModes.none;
        private string m_nodeKey = "";
        private string m_functionToCall = "";

        private bool m_isInitialized = false;

        private XmlTree m_xTree = new XmlTree();

        /// <summary>
        /// Provides easy access to the ServiceContext
        /// </summary>
        protected internal ServiceContext Services
        {
            get { return ApplicationContext.Current.Services; }
        }

        /// <summary>
        /// Initializes the class if it hasn't been done already
        /// </summary>
        protected void Initialize()
        {
            if (!m_isInitialized)
            {
                //VERY IMPORTANT! otherwise it will go infinite loop!
                m_isInitialized = true;

                CreateAllowedActions(); //first create the allowed actions
                
                //raise the event, allow developers to modify the collection
                var nodeActions = new NodeActionsEventArgs(false, m_allowedActions);
                OnNodeActionsCreated(nodeActions);
                m_allowedActions = nodeActions.AllowedActions;

                CreateRootNodeActions();//then create the root node actions

                var rootActions = new NodeActionsEventArgs(true, m_initActions);
                OnNodeActionsCreated(rootActions);
                m_initActions = rootActions.AllowedActions;

                CreateRootNode(); //finally, create the root node itself
            }            
        }

        /// <summary>
        /// This method creates the Root node definition for the tree.
        /// Inheritors must override this method to create their own definition.
        /// </summary>
        /// <param name="rootNode"></param>
        protected abstract void CreateRootNode(ref XmlTreeNode rootNode);
        protected void CreateRootNode()
        {
            m_initNode = XmlTreeNode.CreateRoot(this);
			m_initNode.Icon = FolderIcon;
			m_initNode.OpenIcon = FolderIconOpen;
            CreateRootNode(ref m_initNode);
        }


        /// <summary>
        /// This method creates the IAction list for the tree's root node.
        /// Inheritors can override this method to create their own Context menu.
        /// </summary>
        /// <param name="actions"></param>
        protected virtual void CreateRootNodeActions(ref List<IAction> actions)
        {
            actions.AddRange(GetDefaultRootNodeActions());
        }
        protected void CreateRootNodeActions()
        {
            CreateRootNodeActions(ref m_initActions);
        }

        /// <summary>
        /// This method creates the AllowedActions IAction list for the tree's nodes.
        /// Inheritors can override this method to create their own Context menu.
        /// </summary>
        /// <param name="actions"></param>
        protected virtual void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Add(ActionDelete.Instance);

            //raise the event, allow developers to modify the collection
            var e = new NodeActionsEventArgs(false, actions);
            OnNodeActionsCreated(e);
            actions = e.AllowedActions;

        }
        protected void CreateAllowedActions()
        {
            CreateAllowedActions(ref m_allowedActions);
        }

		/// <summary>		
		/// A helper method to re-generate the root node for the current tree.
		/// </summary>
		/// <returns></returns>
		public XmlTreeNode GenerateRootNode()
		{
			XmlTreeNode node = XmlTreeNode.CreateRoot(this);
			this.CreateRootNode(ref node);
			return node;
		}

        /// <summary>
        /// This method can initialize the ITreeService parameters for this class with another ITreeService object.
        /// This method could be used for Dependency Injection.
        /// </summary>
        /// <param name="treeParams"></param>
        public void SetTreeParameters(ITreeService treeParams)
        {
            this.DialogMode = treeParams.DialogMode;
            this.NodeKey = treeParams.NodeKey;
            this.FunctionToCall = treeParams.FunctionToCall;
            this.IsDialog = treeParams.IsDialog;
            this.ShowContextMenu = treeParams.ShowContextMenu;
            this.id = treeParams.StartNodeID;

            if (!treeParams.ShowContextMenu)
                this.RootNode.Menu = null;
        }

        /// <summary>
        /// Returns the tree service url to render the tree
        /// </summary>
        /// <returns></returns>
        public string GetTreeInitUrl()
        {
            TreeService treeSvc = new TreeService(this.StartNodeID, TreeAlias, null, null, TreeDialogModes.none, "");
            return treeSvc.GetInitUrl();
        }

        /// <summary>
        /// Returns the tree service url to return the tree xml structure from the root node
        /// </summary>
        /// <returns></returns>
        public string GetTreeServiceUrl()
        {
            return GetTreeServiceUrl(this.StartNodeID);
        }

        /// <summary>
        /// Returns the tree service url to return the tree xml structure from the node passed in
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTreeServiceUrl(int id)
        {
			// updated by NH to pass showcontextmenu, isdialog and dialogmode variables
			TreeService treeSvc = new TreeService(id, TreeAlias, this.ShowContextMenu, this.IsDialog, this.DialogMode, "");
			return treeSvc.GetServiceUrl();            
        }

		/// <summary>
		/// Returns the tree service url to return the tree xml structure based on a string node key.
		/// </summary>
		/// <param name="nodeKey"></param>
		/// <returns></returns>
		public string GetTreeServiceUrl(string nodeKey)
		{
			TreeService treeSvc = new TreeService(-1, TreeAlias, this.ShowContextMenu, this.IsDialog, this.DialogMode, "", nodeKey);
			return treeSvc.GetServiceUrl();
		}



        /// <summary>
        /// Returns the tree service url to render the tree in dialog mode
        /// </summary>
        /// <returns></returns>
        public virtual string GetTreeDialogUrl()
        {
            TreeService treeSvc = new TreeService(this.StartNodeID, TreeAlias, false, true, this.DialogMode, "");
            return treeSvc.GetServiceUrl();
        }

        /// <summary>
        /// Returns the tree service url to render tree xml structure from the node passed in, in dialog mode.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual string GetTreeDialogUrl(int id)
        {
            TreeService treeSvc = new TreeService(id, TreeAlias, false, true, this.DialogMode, "");
            return treeSvc.GetServiceUrl();
        }

		/// <summary>
		/// Returns the serialized data for the nodeId passed in.
		/// </summary>
		/// <remarks>
		/// This may not work with ITrees that don't support the BaseTree structure with TreeService.
		/// If a tree implements other query string data to make it work, this may not function since
		/// it only relies on the 3 parameters.
		/// </remarks>
		/// <param name="alias"></param>
		/// <param name="nodeId"></param>
		/// <returns></returns>
		public string GetSerializedNodeData(string nodeId)
		{
			XmlTree xTree = new XmlTree();
			int id;
			if (int.TryParse(nodeId, out id))
				this.id = id;
			else
				this.NodeKey = nodeId;

			this.Render(ref xTree);            

			return xTree.ToString();
		}

        /// <summary>
        /// Returns a boolean value indicating if the ITree passed in is an extension of BaseTree.
        /// This is used to preserve backwards compatibility previous to version 5.
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static bool IsBaseTree(ITree tree)
        {
            return typeof(BaseTree).IsAssignableFrom(tree.GetType());
        }

        /// <summary>
        /// Converts an ITree into a BaseTree. This is used for Legacy trees that don't inherit from BaseTree already.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="alias"></param>
        /// <param name="appAlias"></param>
        /// <param name="iconClosed"></param>
        /// <param name="iconOpened"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static BaseTree FromITree(ITree tree, string alias, string appAlias, string iconClosed, string iconOpened, string action)
        {
            TreeService treeSvc = new TreeService(null, alias, null, null, TreeDialogModes.none, appAlias);
            //create the generic XmlTreeNode and fill it with the properties from the db          
			NullTree nullTree = new NullTree(appAlias);
            XmlTreeNode node = XmlTreeNode.CreateRoot(nullTree);
            node.Text = BaseTree.GetTreeHeader(alias);;
            node.Action = action;
            node.Source = treeSvc.GetServiceUrl();
            node.Icon = iconClosed;
            node.OpenIcon = iconOpened;
            node.NodeType = "init" + alias;
			node.NodeType = alias;
            node.NodeID = "init";
            node.Menu = BaseTree.GetDefaultRootNodeActions();

            //convert the tree to a LegacyTree
            LegacyTree bTree = new LegacyTree(tree, appAlias, node);

            return bTree;
        }

        /// <summary>
        /// Returns the default actions for a root node
        /// </summary>
        /// <returns></returns>
        public static List<IAction> GetDefaultRootNodeActions()
        {
            List<IAction> actions = new List<IAction>();
            actions.Add(ActionNew.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionRefresh.Instance);
            return actions;
        }

        /// <summary>
        /// Returns the tree header title. If the alias isn't found in the language files, then it will
        /// return the title stored in the umbracoAppTree table.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static string GetTreeHeader(string alias)
        {
            string treeCaption = ui.Text(alias);
            //this is a hack. the tree header title should be in the language files, however, if it is not, we're just
            //going to make it equal to what is specified in the db.
            if (treeCaption.Length > 0 && treeCaption.Substring(0, 1) == "[")
            {
                ApplicationTree tree = ApplicationTree.getByAlias(alias);
                if (tree != null)
                    return tree.Title.SplitPascalCasing().ToFirstUpperInvariant();
            }
            return treeCaption;
        }


        #region Events
        
        //These events are poorly designed because they cannot be implemented in the tree inheritance structure,
        //it would be up to the individual trees to ensure they launch the events which is not ideal.
        //they are also named in appropriately in regards to standards and because everything is by ref, there is no need to 
        //have 2 events, makes no difference if you want to modify the contents of the data.
        public delegate void BeforeNodeRenderEventHandler(ref XmlTree sender, ref XmlTreeNode node, EventArgs e);
        public delegate void AfterNodeRenderEventHandler(ref XmlTree sender, ref XmlTreeNode node, EventArgs e);
        public static event BeforeNodeRenderEventHandler BeforeNodeRender;
        public static event AfterNodeRenderEventHandler AfterNodeRender;

        public static event EventHandler<TreeEventArgs> BeforeTreeRender;
        public static event EventHandler<TreeEventArgs> AfterTreeRender;

        public static event EventHandler<TreeEventArgs> BeforeTreeRenderOptimizedMode;
        public static event EventHandler<TreeEventArgs> AfterTreeRenderOptimizedMode;

        /// <summary>
        /// Raises the <see cref="E:BeforeNodeRender"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeNodeRender(ref XmlTree sender, ref XmlTreeNode node, EventArgs e)
        {
            if (node != null && node != null)
            {
                if (BeforeNodeRender != null)
                    BeforeNodeRender(ref sender, ref node, e);    
            }
        }

        /// <summary>
        /// Raises the <see cref="E:AfterNodeRender"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterNodeRender(ref XmlTree sender, ref XmlTreeNode node, EventArgs e)
        {
            if (AfterNodeRender != null)
                AfterNodeRender(ref sender, ref node, e);
        }

        protected virtual void OnBeforeTreeRender(object sender, TreeEventArgs e)
        {
            if (BeforeTreeRender != null)
                BeforeTreeRender(sender, e);
        }

        protected virtual void OnAfterTreeRender(object sender, TreeEventArgs e)
        {
            if (AfterTreeRender != null)
                AfterTreeRender(sender, e);
        }

        protected virtual void OnBeforeTreeRenderOptimizedMode(object sender, TreeEventArgs e)
        {
            if (BeforeTreeRenderOptimizedMode != null)
                BeforeTreeRenderOptimizedMode(sender, e);
        }

        protected virtual void OnAfterTreeRenderOptimizedMode(object sender, TreeEventArgs e)
        {
            if (AfterTreeRenderOptimizedMode != null)
                AfterTreeRenderOptimizedMode(sender, e);
        }

        [Obsolete("Do not use this method to raise events, it is no longer used and will cause very high performance spikes!")]
        protected internal virtual void OnBeforeTreeRender(IEnumerable<IUmbracoEntity> sender, TreeEventArgs e, bool isContent)
        {
            if (BeforeTreeRender != null)
            {
                if (isContent)
                {
                    BeforeTreeRender(sender.Select(x => new Document(x, false)).ToArray(), e);
                }
                else
                {
                    BeforeTreeRender(sender.Select(x => new Media(x, false)).ToArray(), e);
                }
            }
        }

        [Obsolete("Do not use this method to raise events, it is no longer used and will cause very high performance spikes!")]
        protected internal virtual void OnAfterTreeRender(IEnumerable<IUmbracoEntity> sender, TreeEventArgs e, bool isContent)
        {
            if (AfterTreeRender != null)
            {
                if (isContent)
                {
                    AfterTreeRender(sender.Select(x => new Document(x, false)).ToArray(), e);
                }
                else
                {
                    AfterTreeRender(sender.Select(x => new Media(x, false)).ToArray(), e);
                }
            }
        }

        /// <summary>
        /// Returns true if there are subscribers to either BeforeTreeRender or AfterTreeRender
        /// </summary>
        internal bool HasEntityBasedEventSubscribers
        {
            get { return BeforeTreeRender != null || AfterTreeRender != null; }
        }

        /// <summary>
        /// Event that is raised once actions are assigned to nodes
        /// </summary>
        public static event EventHandler<NodeActionsEventArgs> NodeActionsCreated;

        protected virtual void OnNodeActionsCreated(NodeActionsEventArgs e)
        {
            if (NodeActionsCreated != null)
                NodeActionsCreated(this, e);
        }

        #endregion
        
        


    }
}