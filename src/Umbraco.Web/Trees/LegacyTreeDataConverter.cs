using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Web;
using System.Web.Http.Routing;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Trees.Menu;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.helpers;
using umbraco.cms.presentation.Trees;
using umbraco.controls.Tree;
using umbraco.interfaces;

namespace Umbraco.Web.Trees
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class LegacyBaseTreeAttribute : Attribute
    {
        public Type BaseTreeType { get; private set; }

        public LegacyBaseTreeAttribute(Type baseTreeType)
        {
            if (!TypeHelper.IsTypeAssignableFrom<BaseTree>(baseTreeType))
            {
                throw new InvalidOperationException("The type for baseTreeType must be assignable from " + typeof(BaseTree));
            }

            BaseTreeType = baseTreeType;
        }
    }

    internal class LegacyBaseTreeWrapper : BaseTree
    {
        
        private readonly string _treeAlias;
        private readonly TreeNodeCollection _children;
        private readonly TreeNode _root;

        public LegacyBaseTreeWrapper(string treeAlias, string application, TreeNode root, TreeNodeCollection children = null)
            : base(application)
        {
            _treeAlias = treeAlias;
            _root = root;
            _children = children;
        }
        
        public override void RenderJS(ref StringBuilder javascript)
        {
            
        }

        public override void Render(ref XmlTree tree)
        {
            foreach (var c in _children)
            {
                var node = XmlTreeNode.Create(this);
                LegacyTreeDataConverter.ConvertToLegacyNode(node, c, _treeAlias);
                node.Source = IsDialog == false ? GetTreeServiceUrl(int.Parse(node.NodeID)) : GetTreeDialogUrl(int.Parse(node.NodeID));
                tree.Add(node);
            }
        }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeID = _root.NodeId;
            rootNode.Icon = _root.IconIsClass ? _root.Icon.EnsureStartsWith('.') : _root.IconFilePath;
            rootNode.HasChildren = _root.HasChildren;
            rootNode.NodeID = _root.NodeId;
            rootNode.Text = _root.Title;
            rootNode.NodeType = _root.NodeType;
            rootNode.OpenIcon = _root.IconIsClass ? _root.Icon.EnsureStartsWith('.') : _root.IconFilePath;
        }

        public override string TreeAlias
        {
            get { return _treeAlias; }
        }
    }

    /// <summary>
    /// Converts the legacy tree data to the new format
    /// </summary>
    internal class LegacyTreeDataConverter
    {

        internal static FormDataCollection ConvertFromLegacyTreeParams(TreeRequestParams treeParams)
        {
            return new FormDataCollection(new Dictionary<string, string>
                {
                    {TreeQueryStringParameters.Application, treeParams.Application},
                    {TreeQueryStringParameters.DialogMode, treeParams.IsDialog.ToString()},
                });
        }

        internal static void ConvertToLegacyNode(XmlTreeNode legacy, TreeNode node, string treeType)
        {
            legacy.Action = node.AdditionalData.ContainsKey("legacyDialogAction") ? node.AdditionalData["legacyDialogAction"].ToString() : "";
            legacy.HasChildren = node.HasChildren;
            legacy.Icon = node.IconIsClass ? node.Icon.EnsureStartsWith('.') : node.IconFilePath;
            legacy.NodeID = node.NodeId;
            legacy.NodeType = node.NodeType;
            legacy.OpenIcon = node.IconIsClass ? node.Icon.EnsureStartsWith('.') : node.IconFilePath;
            legacy.Text = node.Title;
            legacy.TreeType = treeType;
        }

        /// <summary>
        /// Gets the menu item collection from a legacy tree node based on it's parent node's child collection
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <param name="xmlTree">The node collection that contains the node id</param>
        /// <param name="currentSection"></param>
        /// <returns></returns>
        internal static MenuItemCollection ConvertFromLegacyMenu(string nodeId, XmlTree xmlTree, string currentSection)
        {
            var xmlTreeNode = xmlTree.treeCollection.FirstOrDefault(x => x.NodeID == nodeId);
            if (xmlTreeNode == null)
            {
                return null;
            }

            return ConvertFromLegacyMenu(xmlTreeNode, currentSection);
        }

        /// <summary>
        /// Gets the menu item collection from a legacy tree node
        /// </summary>
        /// <param name="xmlTreeNode"></param>
        /// <param name="currentSection"></param>
        /// <returns></returns>
        internal static MenuItemCollection ConvertFromLegacyMenu(XmlTreeNode xmlTreeNode, string currentSection)
        {
            var collection = new MenuItemCollection();

            var menuItems = xmlTreeNode.Menu.ToArray();
            var numAdded = 0;
            var seperators = new List<int>();
            foreach (var t in menuItems)
            {
                if (t is ContextMenuSeperator && numAdded > 0)
                {
                    //store the index for which the seperator should be placed
                    seperators.Add(collection.Count());
                }
                else
                {
                    var menuItem = collection.AddMenuItem(t);

                    var currentAction = t;

                    //First try to get a URL/title from the legacy action,
                    // if that doesn't work, try to get the legacy confirm view
                    Attempt<LegacyUrlAction>
                        .Try(GetUrlAndTitleFromLegacyAction(currentAction, xmlTreeNode.NodeID, xmlTreeNode.NodeType, xmlTreeNode.Text, currentSection),
                             action => menuItem.LaunchDialogUrl(action.Url, action.DialogTitle))
                        .IfFailed(() => GetLegacyConfirmView(currentAction, currentSection),
                                  view => menuItem.LaunchDialogView(
                                      view, 
                                      ui.GetText("defaultdialogs", "confirmdelete") + " '" + xmlTreeNode.Text + "' ?"));
                    
                    numAdded++;
                }
            }
            var length = collection.Count();
            foreach (var s in seperators)
            {
                if (length >= s)
                {
                    collection.ElementAt(s).SeperatorBefore = true;
                }
            }

            return collection;
        }

        

        /// <summary>
        /// This will look at the legacy IAction's JsFunctionName and convert it to a confirmation dialog view if possible
        /// </summary>
        /// <param name="action"></param>
        /// <param name="currentSection"></param>
        /// <returns></returns>
        internal static Attempt<string> GetLegacyConfirmView(IAction action, string currentSection)
        {
            if (action.JsFunctionName.IsNullOrWhiteSpace())
            {
                return Attempt<string>.False;
            }

            switch (action.JsFunctionName)
            {
                case "UmbClientMgr.appActions().actionDelete()":
                    return new Attempt<string>(
                        true,
                        Core.Configuration.GlobalSettings.Path.EnsureEndsWith('/') + "views/common/dialogs/legacydelete.html");
            }

            return Attempt<string>.False;
        }

        /// <summary>
        /// This will look at a legacy IAction's JsFunctionName and convert it to a URL if possible.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="nodeName"></param>
        /// <param name="currentSection"></param>
        /// <param name="nodeId"></param>
        /// <param name="nodeType"></param>
        internal static Attempt<LegacyUrlAction> GetUrlAndTitleFromLegacyAction(IAction action, string nodeId, string nodeType, string nodeName, string currentSection)
        {
            if (action.JsFunctionName.IsNullOrWhiteSpace())
            {
                return Attempt<LegacyUrlAction>.False;
            }

            switch (action.JsFunctionName)
            {
                case "UmbClientMgr.appActions().actionNew()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "create.aspx?nodeId=" + nodeId + "&nodeType=" + nodeType + "&nodeName=" + nodeName + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "create")));
                case "UmbClientMgr.appActions().actionNewFolder()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "createFolder.aspx?nodeId=" + nodeId + "&nodeType=" + nodeType + "&nodeName=" + nodeName + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "create")));
                case "UmbClientMgr.appActions().actionSort()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/sort.aspx?id=" + nodeId + "&nodeType=" + nodeType + "&app=" + currentSection + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "sort")));
                case "UmbClientMgr.appActions().actionRights()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/cruds.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "rights")));
                case "UmbClientMgr.appActions().actionProtect()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/protectPage.aspx?mode=cut&nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "protect")));
                case "UmbClientMgr.appActions().actionRollback()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/rollback.aspx?nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "rollback")));
                case "UmbClientMgr.appActions().actionNotify()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/notifications.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "notify")));
                case "UmbClientMgr.appActions().actionPublish()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/publish.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "publish")));
                case "UmbClientMgr.appActions().actionToPublish()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/SendPublish.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "sendtopublish")));
                case "UmbClientMgr.appActions().actionRePublish()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/republish.aspx?rnd=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            "Republishing entire site"));
                case "UmbClientMgr.appActions().actionAssignDomain()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/assignDomain2.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "assignDomain")));
                case "UmbClientMgr.appActions().actionLiveEdit()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "canvas.aspx?redir=/" + nodeId + ".aspx",
                            "",
                            ActionUrlMethod.BlankWindow));
                case "UmbClientMgr.appActions().actionSendToTranslate()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/sendToTranslation.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "sendToTranslate")));
                case "UmbClientMgr.appActions().actionEmptyTranscan()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/emptyTrashcan.aspx?type=" + currentSection,
                            ui.GetText("actions", "emptyTrashcan")));
                case "UmbClientMgr.appActions().actionImport()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/importDocumentType.aspx",
                            ui.GetText("actions", "importDocumentType")));
                case "UmbClientMgr.appActions().actionExport()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/exportDocumentType.aspx?nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ""));
                case "UmbClientMgr.appActions().actionAudit()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/viewAuditTrail.aspx?nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "auditTrail")));
                case "UmbClientMgr.appActions().actionMove()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/moveOrCopy.aspx?app=" + currentSection + "&mode=cut&id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "move")));
                case "UmbClientMgr.appActions().actionCopy()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/moveOrCopy.aspx?app=" + currentSection + "&mode=copy&id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "copy")));
            }
            return Attempt<LegacyUrlAction>.False;
        }

        internal static TreeNode ConvertFromLegacy(string parentId, XmlTreeNode xmlTreeNode, UrlHelper urlHelper, string currentSection, bool isRoot = false)
        {
            //  /umbraco/tree.aspx?rnd=d0d0ff11a1c347dabfaa0fc75effcc2a&id=1046&treeType=content&contextMenu=false&isDialog=false

            //we need to convert the node source to our legacy tree controller
            var childNodesSource = urlHelper.GetUmbracoApiService<LegacyTreeController>("GetNodes");

            var childQuery = (xmlTreeNode.Source.IsNullOrWhiteSpace() || xmlTreeNode.Source.IndexOf('?') == -1)
                ? ""
                : xmlTreeNode.Source.Substring(xmlTreeNode.Source.IndexOf('?'));

            //append the query strings
            childNodesSource = childNodesSource.AppendQueryStringToUrl(childQuery);

            //for the menu source we need to detect if this is a root node since we'll need to set the parentId and id to -1
            // for which we'll handle correctly on the server side.            
            var menuSource = urlHelper.GetUmbracoApiService<LegacyTreeController>("GetMenu");
            menuSource = menuSource.AppendQueryStringToUrl(new[]
                {
                    "id=" + (isRoot ? "-1" : xmlTreeNode.NodeID),
                    "treeType=" + xmlTreeNode.TreeType,
                    "parentId=" + (isRoot ? "-1" : parentId),
                    "section=" + currentSection
                });

            //TODO: Might need to add stuff to additional attributes

            var node = new TreeNode(xmlTreeNode.NodeID, childNodesSource, menuSource)
            {
                HasChildren = xmlTreeNode.HasChildren,
                Icon = xmlTreeNode.Icon,
                Title = xmlTreeNode.Text,
                NodeType = xmlTreeNode.NodeType
            };
            if (isRoot)
            {
                node.AdditionalData.Add("treeAlias", xmlTreeNode.TreeType);
            }

            //This is a special case scenario, we know that content/media works based on the normal Belle routing/editing so we'll ensure we don't
            // pass in the legacy JS handler so we do it the new way, for all other trees (Currently, this is a WIP), we'll render
            // the legacy js callback,.
            var knownNonLegacyNodeTypes = new[] { "content", "contentRecycleBin", "mediaRecyleBin", "media" };
            if (knownNonLegacyNodeTypes.InvariantContains(xmlTreeNode.NodeType) == false)
            {
                node.AssignLegacyJsCallback(xmlTreeNode.Action);
            }
            return node;
        }

        internal static TreeNodeCollection ConvertFromLegacy(string parentId, XmlTree xmlTree, UrlHelper urlHelper, string currentSection)
        {
            //TODO: Once we get the editor URL stuff working we'll need to figure out how to convert 
            // that over to use the old school ui.xml stuff for these old trees and however the old menu items worked.

            var collection = new TreeNodeCollection();
            foreach (var x in xmlTree.treeCollection)
            {
                collection.Add(ConvertFromLegacy(parentId, x, urlHelper, currentSection));
            }
            return collection;
        }

        internal class LegacyUrlAction
        {
            public LegacyUrlAction(string url, string dialogTitle)
                : this(url, dialogTitle, ActionUrlMethod.Dialog)
            {

            }

            public LegacyUrlAction(string url, string dialogTitle, ActionUrlMethod actionMethod)
            {
                Url = url;
                ActionMethod = actionMethod;
                DialogTitle = dialogTitle;
            }

            public string Url { get; private set; }
            public ActionUrlMethod ActionMethod { get; private set; }
            public string DialogTitle { get; private set; }
        }

    }
}