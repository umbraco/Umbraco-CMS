using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;
using Umbraco.Core;
using Action = umbraco.BusinessLogic.Actions.Action;

namespace umbraco
{
    /// <summary>
    /// Handles loading the content tree into umbraco's application tree
    /// </summary>
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    //[Tree(Constants.Applications.Content, "content", "Content", silent: true)]
    public class loadContent : BaseContentTree
    {

        public loadContent(string application)
            : base(application)
        {
            this._StartNodeID = CurrentUser.StartNodeId;
        }

        private Document m_document;
        private int _StartNodeID;

        /// <summary>
        /// Returns the Document object of the starting node for the current User. This ensures
        /// that the Document object is only instantiated once.
        /// </summary>
        protected Document StartNode
        {
            get
            {
                if (m_document == null)
                {
                    m_document = new Document(StartNodeID);
                }

                if (!m_document.Path.Contains(CurrentUser.StartNodeId.ToString()))
                {
                    var doc = new Document(CurrentUser.StartNodeId);
                    if (!string.IsNullOrEmpty(doc.Path) && doc.Path.Contains(this.StartNodeID.ToString()))
                    {
                        m_document = doc;
                    }
                    else
                    {
                        return null;
                    }
                }

                return m_document;
            }
        }

        protected override bool LoadMinimalDocument
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Creates the root node context menu for the content tree.
        /// Depending on the current User's permissions, this menu will change.
        /// If the current User's starting node is not -1 (the normal root content tree node)
        /// then the menu will be built based on the permissions of the User's start node.
        /// </summary>
        /// <param name="actions"></param>
        protected override void CreateRootNodeActions(ref List<IAction> actions)
        {
            actions.Clear();

            if (StartNodeID != -1)
            {
                //get the document for the start node id
                Document doc = StartNode;
                if (doc == null)
                {
                    return;
                }
                //get the allowed actions for the user for the current node
                List<IAction> nodeActions = GetUserActionsForNode(doc);
                //get the allowed actions for the tree based on the users allowed actions
                List<IAction> allowedMenu = GetUserAllowedActions(AllowedActions, nodeActions);
                actions.AddRange(allowedMenu);
            }
            else
            {
                // we need to get the default permissions as you can't set permissions on the very root node
                List<IAction> nodeActions = Action.FromString(CurrentUser.GetPermissions("-1"));
                List<IAction> allowedRootActions = new List<IAction>();
                allowedRootActions.Add(ActionNew.Instance);
                allowedRootActions.Add(ActionSort.Instance);
                List<IAction> allowedMenu = GetUserAllowedActions(allowedRootActions, nodeActions);
                actions.AddRange(allowedMenu);
                if (allowedMenu.Count > 0)
                    actions.Add(ContextMenuSeperator.Instance);

                // default actions for all users
                actions.Add(ActionRePublish.Instance);
                actions.Add(ContextMenuSeperator.Instance);
                actions.Add(ActionRefresh.Instance);
                //actions.Add(ActionTreeEditMode.Instance);
            }
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionNew.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionDelete.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionMove.Instance);
            actions.Add(ActionCopy.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionSort.Instance);
            actions.Add(ActionRollback.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionChangeDocType.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionPublish.Instance);
            actions.Add(ActionToPublish.Instance);
            actions.Add(ActionAssignDomain.Instance);
            actions.Add(ActionRights.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionProtect.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionUnPublish.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionNotify.Instance);
            actions.Add(ActionSendToTranslate.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionRefresh.Instance);
        }

        /// <summary>
        /// Creates the root node for the content tree. If the current User does
        /// not have access to the actual content tree root, then we'll display the 
        /// node that correlates to their StartNodeID
        /// </summary>
        /// <param name="rootNode"></param>
        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            if (StartNodeID != -1)
            {
                Document doc = StartNode;
                if (doc == null)
                {
                    rootNode = new NullTree(this.app).RootNode;
                    rootNode.Text = "You do not have permission for this content tree";
                    rootNode.HasChildren = false;
                    rootNode.Source = string.Empty;
                }
                else
                {
                    rootNode = CreateNode(doc, RootNodeActions);
                }
            }
            else
            {
                if (IsDialog)
                    rootNode.Action = "javascript:openContent(-1);";
            }

        }

        /// <summary>
        /// If the user is an admin, always return entire tree structure, otherwise
        /// return the user's start node id.
        /// </summary>
        public override int StartNodeID
        {
            get
            {
                return this._StartNodeID;
            }
        }

        /// <summary>
        /// Adds the recycling bin node. This method should only actually add the recycle bin node when the tree is initially created and if the user
        /// actually has access to the root node.
        /// </summary>
        /// <returns></returns>
        protected XmlTreeNode CreateRecycleBin()
        {
            if (m_id == -1 && !this.IsDialog)
            {
                //create a new content recycle bin tree, initialized with it's startnodeid
                ContentRecycleBin bin = new ContentRecycleBin(this.m_app);
                bin.ShowContextMenu = this.ShowContextMenu;
                bin.id = bin.StartNodeID;
                return bin.RootNode;
            }
            return null;
        }


        /// <summary>
        /// Override the render method to add the recycle bin to the end of this tree
        /// </summary>
        /// <param name="Tree"></param>
        public override void Render(ref XmlTree tree)
        {
            base.Render(ref tree);
            XmlTreeNode recycleBin = CreateRecycleBin();
            if (recycleBin != null)
                tree.Add(recycleBin);
        }
    }
}
