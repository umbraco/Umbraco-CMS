using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.BusinessLogic.Actions;


namespace umbraco.cms.presentation.Trees
{
    /// <summary>
    /// An abstract tree class for the content application.
    /// Has built in methods for handling all of the request parameters specific to the content tree.
    /// </summary>
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public abstract class BaseContentTree : BaseTree
    {

        public BaseContentTree(string application) : base(application) { }

        private User _user;
        
        protected virtual bool LoadMinimalDocument { get; set; }

        /// <summary>
        /// Returns the current User. This ensures that we don't instantiate a new User object 
        /// each time.
        /// </summary>
        protected User CurrentUser
        {
            get
            {
                return (_user == null ? (_user = UmbracoEnsuredPage.CurrentUser) : _user);
            }
        }

        /// <summary>
        /// Renders the Javascript.
        /// </summary>
        /// <param name="Javascript">The javascript.</param>
        public override void RenderJS(ref StringBuilder Javascript)
        {
            if (!String.IsNullOrEmpty(this.FunctionToCall))
            {
                Javascript.Append("function openContent(id) {\n");
                Javascript.Append(this.FunctionToCall + "(id);\n");
                Javascript.Append("}\n");
            }
            else if (!this.IsDialog)
            {
                Javascript.Append(
					@"
function openContent(id) {
	UmbClientMgr.contentFrame('editContent.aspx?id=' + id);
}
");
            }
        }

        /// <summary>
        /// Renders the specified tree item.
        /// </summary>        
        /// <param name="Tree">The tree.</param>
        public override void Render(ref XmlTree Tree)
        {
            if (UseOptimizedRendering == false)
            {
                //We cannot run optimized mode since there are subscribers to events/methods that require document instances
                // so we'll render the original way by looking up the docs.

                //get documents to render
                var docs = Document.GetChildrenForTree(m_id);

                var args = new TreeEventArgs(Tree);
                OnBeforeTreeRender(docs, args);

                foreach (var dd in docs)
                {
                    var allowedUserOptions = GetUserActionsForNode(dd);
                    if (CanUserAccessNode(dd, allowedUserOptions))
                    {

                        var node = CreateNode(dd, allowedUserOptions);

                        OnRenderNode(ref node, dd);

                        OnBeforeNodeRender(ref Tree, ref node, EventArgs.Empty);
                        if (node != null)
                        {
                            Tree.Add(node);
                            OnAfterNodeRender(ref Tree, ref node, EventArgs.Empty);
                        }
                    }
                }
                OnAfterTreeRender(docs, args);
            }
            else
            {

                //We ARE running in optmized mode, this means we will NOT be raising the BeforeTreeRender or AfterTreeRender 
                // events and NOT calling the OnRenderNode method - we've already detected that there are not subscribers or implementations
                // to call so that is fine.

                var entities = Services.EntityService.GetChildren(m_id, UmbracoObjectTypes.Document).ToArray();
                foreach (var entity in entities)
                {
                    var e = entity as UmbracoEntity;
                    var allowedUserOptions = GetUserActionsForNode(e);
                    if (CanUserAccessNode(e, allowedUserOptions))
                    {
                        var node = CreateNode(e, allowedUserOptions);

                        //in optimized mode the LoadMinimalDocument will ALWAYS be true, if it is not true then we will
                        // be rendering in non-optimized mode and this code will not get executed so we don't need to worry
                        // about performance here.
                        OnRenderNode(ref node, new Document(e, LoadMinimalDocument));
                        
                        OnBeforeNodeRender(ref Tree, ref node, EventArgs.Empty);
                        if (node != null)
                        {
                            Tree.Add(node);
                            OnAfterNodeRender(ref Tree, ref node, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        #region Tree Create-node-helper Methods - Legacy
        /// <summary>
        /// Creates an XmlTreeNode based on the passed in Document
        /// </summary>
        /// <param name="dd"></param>
        /// <param name="allowedUserOptions"></param>
        /// <returns></returns>
        protected XmlTreeNode CreateNode(Document dd, List<IAction> allowedUserOptions)
        {
            XmlTreeNode node = XmlTreeNode.Create(this);
            SetMenuAttribute(ref node, allowedUserOptions);
            node.NodeID = dd.Id.ToString();
            node.Text = dd.Text;
            SetNonPublishedAttribute(ref node, dd);
            SetProtectedAttribute(ref node, dd);
            SetActionAttribute(ref node, dd);
            SetSourcesAttributes(ref node, dd);
            if (dd.ContentTypeIcon != null)
            {
                node.Icon = dd.ContentTypeIcon;
                node.OpenIcon = dd.ContentTypeIcon;
            }

            if (dd.HasPublishedVersion() == false)
                node.Style.DimNode();

            if (dd.HasPendingChanges())
                node.Style.HighlightNode();

            return node;
        }
        
        /// <summary>
        /// Creates the link for the current document 
        /// </summary>
        /// <param name="dd"></param>
        /// <returns></returns>
        protected string CreateNodeLink(Document dd)
        {
            string nodeLink = library.NiceUrl(dd.Id);
            if (nodeLink == "")
            {
                nodeLink = "/" + dd.Id;
                if (GlobalSettings.UseDirectoryUrls == false)
                    nodeLink += ".aspx";
            }
            return nodeLink;
        }

        /// <summary>
        /// Inheritors override this method to modify the content node being created
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="doc"></param>
        protected virtual void OnRenderNode(ref XmlTreeNode xNode, Document doc) { }

        /// <summary>
        /// Determins if the user has access to view the node/document
        /// </summary>
        /// <param name="doc">The Document to check permissions against</param>
        /// <param name="allowedUserOptions">A list of IActions that the user has permissions to execute on the current document</param>
        /// <remarks>By default the user must have Browse permissions to see the node in the Content tree</remarks>
        /// <returns></returns>        
        protected virtual bool CanUserAccessNode(Document doc, List<IAction> allowedUserOptions)
        {
            if (allowedUserOptions.Contains(ActionBrowse.Instance))
                return true;

            return false;
        }

        /// <summary>
        /// Builds a string of actions that the user is able to perform on the current document.
        /// The list of actions is subject to the user's rights assigned to the document and also
        /// is dependant on the type of node.
        /// </summary>
        /// <param name="dd"></param>
        /// <returns></returns>
        protected List<IAction> GetUserActionsForNode(Document dd)
        {
            List<IAction> actions = umbraco.BusinessLogic.Actions.Action.FromString(CurrentUser.GetPermissions(dd.Path));

            // A user is allowed to delete their own stuff
            if (dd.UserId == CurrentUser.Id && actions.Contains(ActionDelete.Instance) == false)
                actions.Add(ActionDelete.Instance);

            return actions;
        }

        #endregion

        #region Tree Create-node-helper Methods - UmbracoEntity equivalent
        /// <summary>
        /// Creates an XmlTreeNode based on the passed in UmbracoEntity
        /// </summary>
        /// <param name="dd"></param>
        /// <param name="allowedUserOptions"></param>
        /// <returns></returns>
        internal XmlTreeNode CreateNode(UmbracoEntity dd, List<IAction> allowedUserOptions)
        {
            XmlTreeNode node = XmlTreeNode.Create(this);
            SetMenuAttribute(ref node, allowedUserOptions);
            node.NodeID = dd.Id.ToString();
            node.Text = dd.Name;
            SetNonPublishedAttribute(ref node, dd);
            SetProtectedAttribute(ref node, dd);
            SetActionAttribute(ref node, dd);
            SetSourcesAttributes(ref node, dd);
            if (dd.ContentTypeIcon != null)
            {
                node.Icon = dd.ContentTypeIcon;
                node.OpenIcon = dd.ContentTypeIcon;
            }

            if (dd.IsPublished == false)
                node.Style.DimNode();

            if (dd.HasPendingChanges)
                node.Style.HighlightNode();

            return node;
        }

        /// <summary>
        /// Creates the link for the current UmbracoEntity 
        /// </summary>
        /// <param name="dd"></param>
        /// <returns></returns>
        internal string CreateNodeLink(UmbracoEntity dd)
        {
            string nodeLink = library.NiceUrl(dd.Id);
            if (nodeLink == "")
            {
                nodeLink = "/" + dd.Id;
                if (GlobalSettings.UseDirectoryUrls == false)
                    nodeLink += ".aspx";
            }
            return nodeLink;
        }

        /// <summary>
        /// Determins if the user has access to view the node/document
        /// </summary>
        /// <param name="doc">The Document to check permissions against</param>
        /// <param name="allowedUserOptions">A list of IActions that the user has permissions to execute on the current document</param>
        /// <remarks>By default the user must have Browse permissions to see the node in the Content tree</remarks>
        /// <returns></returns>        
        internal virtual bool CanUserAccessNode(UmbracoEntity doc, List<IAction> allowedUserOptions)
        {
            if (allowedUserOptions.Contains(ActionBrowse.Instance))
                return true;

            return false;
        }

        /// <summary>
        /// Builds a string of actions that the user is able to perform on the current document.
        /// The list of actions is subject to the user's rights assigned to the document and also
        /// is dependant on the type of node.
        /// </summary>
        /// <param name="dd"></param>
        /// <returns></returns>
        internal List<IAction> GetUserActionsForNode(UmbracoEntity dd)
        {
            List<IAction> actions = umbraco.BusinessLogic.Actions.Action.FromString(CurrentUser.GetPermissions(dd.Path));

            // A user is allowed to delete their own stuff
            if (dd.CreatorId == CurrentUser.Id && actions.Contains(ActionDelete.Instance) == false)
                actions.Add(ActionDelete.Instance);

            return actions;
        }
        #endregion

        #region Tree Attribute Setter Methods - Legacy

        protected void SetNonPublishedAttribute(ref XmlTreeNode treeElement, Document dd)
        {
            treeElement.NotPublished = false;
            if (dd.Published)
            {
                //if (Math.Round(new TimeSpan(dd.UpdateDate.Ticks - dd.VersionDate.Ticks).TotalSeconds, 0) > 1)
                //    treeElement.NotPublished = true;
                treeElement.NotPublished = dd.HasPendingChanges();
            }
            else
                treeElement.NotPublished = true;
        }
        
        protected void SetProtectedAttribute(ref XmlTreeNode treeElement, Document dd)
        {
            if (Access.IsProtected(dd.Id, dd.Path))
                treeElement.IsProtected = true;
            else
                treeElement.IsProtected = false;
        }
        
        protected void SetActionAttribute(ref XmlTreeNode treeElement, Document dd)
        {
            
            // Check for dialog behaviour
            if (this.DialogMode == TreeDialogModes.fulllink )
            {
                string nodeLink = CreateNodeLink(dd);
                treeElement.Action = String.Format("javascript:openContent('{0}');", nodeLink);
            }
            else if (this.DialogMode == TreeDialogModes.locallink)
            {
                string nodeLink = string.Format("{{localLink:{0}}}", dd.Id);
                string nodeText = dd.Text.Replace("'", "\\'");
                // try to make a niceurl too
                string niceUrl = umbraco.library.NiceUrl(dd.Id).Replace("'", "\\'"); ;
                if (niceUrl != "#" || niceUrl != "") {
                    nodeLink += "|" + niceUrl + "|" + HttpContext.Current.Server.HtmlEncode(nodeText);
                } else {
                    nodeLink += "||" + HttpContext.Current.Server.HtmlEncode(nodeText);
                }
                
                treeElement.Action = String.Format("javascript:openContent('{0}');", nodeLink);
            }
            else if (this.DialogMode == TreeDialogModes.id || this.DialogMode == TreeDialogModes.none)
            {
                treeElement.Action = String.Format("javascript:openContent('{0}');", dd.Id.ToString());
            }
            else if (!this.IsDialog || (this.DialogMode == TreeDialogModes.id))
            {
                if (CurrentUser.GetPermissions(dd.Path).Contains(ActionUpdate.Instance.Letter.ToString()))
                {
                    treeElement.Action = String.Format("javascript:openContent({0});", dd.Id);
                }
            }
        }
        
        protected void SetSourcesAttributes(ref XmlTreeNode treeElement, Document dd)
        {
            treeElement.HasChildren = dd.ContentType.IsContainerContentType == false && dd.HasChildren;
            treeElement.Source = IsDialog == false ? GetTreeServiceUrl(dd.Id) : GetTreeDialogUrl(dd.Id);
        }

        protected void SetMenuAttribute(ref XmlTreeNode treeElement, List<IAction> allowedUserActions)
        {
            //clear menu if we're to hide it
            treeElement.Menu = this.ShowContextMenu == false ? null : RemoveDuplicateMenuDividers(GetUserAllowedActions(AllowedActions, allowedUserActions));
        }

        #endregion

        #region Tree Attribute Setter Methods - UmbracoEntity equivalent

        internal void SetNonPublishedAttribute(ref XmlTreeNode treeElement, UmbracoEntity dd)
        {
            treeElement.NotPublished = false;
            if (dd.IsPublished)
                treeElement.NotPublished = dd.HasPendingChanges;
            else
                treeElement.NotPublished = true;
        }

        internal void SetProtectedAttribute(ref XmlTreeNode treeElement, UmbracoEntity dd)
        {
            if (Access.IsProtected(dd.Id, dd.Path))
                treeElement.IsProtected = true;
            else
                treeElement.IsProtected = false;
        }

        internal void SetActionAttribute(ref XmlTreeNode treeElement, UmbracoEntity dd)
        {

            // Check for dialog behaviour
            if (this.DialogMode == TreeDialogModes.fulllink)
            {
                string nodeLink = CreateNodeLink(dd);
                treeElement.Action = String.Format("javascript:openContent('{0}');", nodeLink);
            }
            else if (this.DialogMode == TreeDialogModes.locallink)
            {
                string nodeLink = string.Format("{{localLink:{0}}}", dd.Id);
                string nodeText = dd.Name.Replace("'", "\\'");
                // try to make a niceurl too
                string niceUrl = umbraco.library.NiceUrl(dd.Id).Replace("'", "\\'"); ;
                if (niceUrl != "#" || niceUrl != "")
                {
                    nodeLink += "|" + niceUrl + "|" + HttpContext.Current.Server.HtmlEncode(nodeText);
                }
                else
                {
                    nodeLink += "||" + HttpContext.Current.Server.HtmlEncode(nodeText);
                }

                treeElement.Action = String.Format("javascript:openContent('{0}');", nodeLink);
            }
            else if (this.DialogMode == TreeDialogModes.id || this.DialogMode == TreeDialogModes.none)
            {
                treeElement.Action = String.Format("javascript:openContent('{0}');", dd.Id.ToString(CultureInfo.InvariantCulture));
            }
            else if (this.IsDialog == false || (this.DialogMode == TreeDialogModes.id))
            {
                if (CurrentUser.GetPermissions(dd.Path).Contains(ActionUpdate.Instance.Letter.ToString(CultureInfo.InvariantCulture)))
                {
                    treeElement.Action = String.Format("javascript:openContent({0});", dd.Id);
                }
            }
        }

        internal void SetSourcesAttributes(ref XmlTreeNode treeElement, UmbracoEntity dd)
        {
            treeElement.HasChildren = dd.HasChildren;
            treeElement.Source = IsDialog == false ? GetTreeServiceUrl(dd.Id) : GetTreeDialogUrl(dd.Id);
        }

        #endregion

        /// <summary>
        /// The returned list is filtered based on the IActions that the user is allowed to perform based on the actions
        /// that are allowed for the current tree.
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="userAllowedActions"></param>
        /// <returns></returns>
        protected List<IAction> GetUserAllowedActions(List<IAction> actions, List<IAction> userAllowedActions)
        {
            return actions.FindAll(
                delegate(IAction a)
                {
                    return (!a.CanBePermissionAssigned || (a.CanBePermissionAssigned && userAllowedActions.Contains(a)));
                }
            );

        }

        /// <summary>
        /// Once the context menu has been created, this utility will simply strip out duplicate dividers if they exist and also leading and trailing dividers.
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        protected List<IAction> RemoveDuplicateMenuDividers(List<IAction> actions)
        {
            string fullMenu = umbraco.BusinessLogic.Actions.Action.ToString(actions);
            while (fullMenu.IndexOf(",,") > 0) //remove all occurances of duplicate dividers
                fullMenu = fullMenu.Replace(",,", ",");
            fullMenu = fullMenu.Trim(new char[] { ',' }); //remove any ending dividers
            return umbraco.BusinessLogic.Actions.Action.FromString(fullMenu);
        }

        /// <summary>
        /// Returns true if we can use the EntityService to render the tree or revert to the original way 
        /// using normal documents
        /// </summary>
        /// <remarks>
        /// We determine this by:
        /// * If there are any subscribers to the events: BeforeTreeRender or AfterTreeRender - then we cannot run optimized
        /// * If there are any overrides of the method: OnRenderNode - then we cannot run optimized
        /// </remarks>
        internal bool UseOptimizedRendering
        {
            get
            {
                if (HasEntityBasedEventSubscribers)
                {
                    return false;
                }

                //now we need to check if the current tree type has OnRenderNode overridden with a custom implementation
                //Strangely - this even works in med trust!
                var method = this.GetType().GetMethod("OnRenderNode", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(XmlTreeNode).MakeByRefType(), typeof(Document) }, null);
                if (TypeHelper.IsOverride(method) && LoadMinimalDocument == false)
                {
                    return false;
                }

                return true;
            }
        }

    }
}
