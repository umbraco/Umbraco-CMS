using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.contentitem;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.BusinessLogic.Actions;


namespace umbraco.cms.presentation.Trees
{
    /// <summary>
    /// An abstract tree class for the content application.
    /// Has built in methods for handling all of the request parameters specific to the content tree.
    /// </summary>
    public abstract class BaseContentTree : BaseTree
    {

        public BaseContentTree(string application) : base(application) { }

        private User m_user;

        /// <summary>
        /// Returns the current User. This ensures that we don't instantiate a new User object 
        /// each time.
        /// </summary>
        protected User CurrentUser
        {
            get
            {
                return (m_user == null ? (m_user = UmbracoEnsuredPage.CurrentUser) : m_user);
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
                Javascript.Append(this.FunctionToCall + "(id)\n");
                Javascript.Append("}\n");
            }
            else if (!this.IsDialog)
            {
                Javascript.Append(
					@"
function openContent(id) {
	" + ClientTools.Scripts.GetContentFrame() + ".location.href = '/umbraco/editContent.aspx?id=' + id;" + @"
}
");
            }
            else
            {
				//TODO: SD: Find out how what this does...?
                Javascript.Append(
                    @"
function openContent(id) {
	if (parent.opener)
		parent.opener.dialogHandler(id);
	else
		parent.dialogHandler(id);	
}

");
            }
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
                if (!GlobalSettings.UseDirectoryUrls)
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
			if (!dd.Published)
				node.Style.DimNode();
            return node;
        }

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
            if (dd.UserId == CurrentUser.Id && !actions.Contains(ActionDelete.Instance))
                actions.Add(ActionDelete.Instance);

            return actions;
        }

        /// <summary>
        /// Renders the specified tree item.
        /// </summary>        
        /// <param name="Tree">The tree.</param>
        public override void Render(ref XmlTree Tree)
        {
            //get documents to render
            Document[] docs = Document.GetChildrenForTree(m_id);

            foreach (Document dd in docs)
            {
                List<IAction> allowedUserOptions = GetUserActionsForNode(dd);
                if (CanUserAccessNode(dd, allowedUserOptions))
                {

                    XmlTreeNode node = CreateNode(dd, allowedUserOptions);

                    OnRenderNode(ref node, dd);

                    base.OnBeforeNodeRender(ref Tree, ref node, EventArgs.Empty);
                    if (node != null)
                    {
                        Tree.Add(node);
                    }
                    base.OnAfterNodeRender(ref Tree, ref node, EventArgs.Empty);
                }
            }
        }

        #region Tree Attribute Setter Methods
        protected void SetNonPublishedAttribute(ref XmlTreeNode treeElement, Document dd)
        {
            treeElement.NotPublished = false;
            if (dd.Published)
            {
                if (Math.Round(new TimeSpan(dd.UpdateDate.Ticks - dd.VersionDate.Ticks).TotalSeconds, 0) > 1)
                    treeElement.NotPublished = true;
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
            if (this.DialogMode == TreeDialogModes.fulllink)
            {
                string nodeLink = CreateNodeLink(dd);
                treeElement.Action = String.Format("javascript:openContent('{0}');", nodeLink);
            }
            else if (this.DialogMode == TreeDialogModes.locallink)
            {
                string nodeLink = string.Format("{{localLink:{0}}}", dd.Id);
                // try to make a niceurl too
                string niceUrl = umbraco.library.NiceUrl(dd.Id);
                if (niceUrl != "#" || niceUrl != "") {
                    nodeLink += "|" + niceUrl + "|" + HttpContext.Current.Server.HtmlEncode(dd.Text);
                } else {
                    nodeLink += "||" + HttpContext.Current.Server.HtmlEncode(dd.Text);
                }
                
                treeElement.Action = String.Format("javascript:openContent('{0}');", nodeLink.Replace("'","\\'"));
            }
            else if (!this.IsDialog || (this.DialogMode == TreeDialogModes.id))
            {
                if (CurrentUser.GetPermissions(dd.Path).Contains(ActionUpdate.Instance.Letter.ToString())) {
                    treeElement.Action = String.Format("javascript:openContent({0});", dd.Id);
                }
            }
        }
        protected void SetSourcesAttributes(ref XmlTreeNode treeElement, Document dd)
        {
			treeElement.HasChildren = dd.HasChildren;
			if (!IsDialog)
				treeElement.Source = GetTreeServiceUrl(dd.Id);
			else
				treeElement.Source = GetTreeDialogUrl(dd.Id);
        }
        protected void SetMenuAttribute(ref XmlTreeNode treeElement, List<IAction> allowedUserActions)
        {
            //clear menu if we're to hide it
            if (!this.ShowContextMenu)
                treeElement.Menu = null;
            else
                treeElement.Menu = RemoveDuplicateMenuDividers(GetUserAllowedActions(AllowedActions, allowedUserActions));

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

    }
}
