using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Routing;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.helpers;
using umbraco.cms.presentation.Trees;
using umbraco.controls.Tree;
using umbraco.interfaces;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Converts the legacy tree data to the new format
    /// </summary>
    internal class LegacyTreeDataConverter
    {
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
                        .Try(GetUrlAndTitleFromLegacyAction(currentAction, xmlTreeNode, currentSection),
                             action => menuItem.LaunchDialogUrl(action.Url, action.DialogTitle))
                        .IfFailed(() => GetLegacyConfirmView(currentAction, xmlTreeNode, currentSection),
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
        /// <param name="actionNode"></param>
        /// <param name="currentSection"></param>
        /// <returns></returns>
        internal static Attempt<string> GetLegacyConfirmView(IAction action, XmlTreeNode actionNode, string currentSection)
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
        /// <param name="actionNode"></param>
        /// <param name="currentSection"></param>
        internal static Attempt<LegacyUrlAction> GetUrlAndTitleFromLegacyAction(IAction action, XmlTreeNode actionNode, string currentSection)
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
                            "create.aspx?nodeId=" + actionNode.NodeID + "&nodeType=" + actionNode.NodeType + "&nodeName=" + actionNode.Text + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "create")));
                case "UmbClientMgr.appActions().actionNewFolder()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "createFolder.aspx?nodeId=" + actionNode.NodeID + "&nodeType=" + actionNode.NodeType + "&nodeName=" + actionNode.Text + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "create")));
                case "UmbClientMgr.appActions().actionSort()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/sort.aspx?id=" + actionNode.NodeID + "&nodeType=" + actionNode.NodeType + "&app=" + currentSection + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "sort")));
                case "UmbClientMgr.appActions().actionRights()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/cruds.aspx?id=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "rights")));
                case "UmbClientMgr.appActions().actionProtect()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/protectPage.aspx?mode=cut&nodeId=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "protect")));
                case "UmbClientMgr.appActions().actionRollback()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/rollback.aspx?nodeId=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "rollback")));
                case "UmbClientMgr.appActions().actionNotify()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/notifications.aspx?id=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "notify")));
                case "UmbClientMgr.appActions().actionPublish()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/publish.aspx?id=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "publish")));
                case "UmbClientMgr.appActions().actionToPublish()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/SendPublish.aspx?id=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "sendtopublish")));
                case "UmbClientMgr.appActions().actionRePublish()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/republish.aspx?rnd=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            "Republishing entire site"));
                case "UmbClientMgr.appActions().actionAssignDomain()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/assignDomain2.aspx?id=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "assignDomain")));
                case "UmbClientMgr.appActions().actionLiveEdit()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "canvas.aspx?redir=/" + actionNode.NodeID + ".aspx",
                            "",
                            ActionUrlMethod.BlankWindow));
                case "UmbClientMgr.appActions().actionSendToTranslate()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/sendToTranslation.aspx?id=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
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
                            "dialogs/exportDocumentType.aspx?nodeId=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ""));
                case "UmbClientMgr.appActions().actionAudit()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/viewAuditTrail.aspx?nodeId=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "auditTrail")));
                case "UmbClientMgr.appActions().actionMove()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/moveOrCopy.aspx?app=" + currentSection + "&mode=cut&id=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "move")));
                case "UmbClientMgr.appActions().actionCopy()":
                    return new Attempt<LegacyUrlAction>(
                        true,
                        new LegacyUrlAction(
                            "dialogs/moveOrCopy.aspx?app=" + currentSection + "&mode=copy&id=" + actionNode.NodeID + "&rnd=" + DateTime.UtcNow.Ticks,
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