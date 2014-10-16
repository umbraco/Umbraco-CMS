using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Web;
using System.Web.Http.Routing;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using umbraco;
using umbraco.BusinessLogic;
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
        internal static BaseTree GetLegacyTreeForLegacyServices(Core.Models.ApplicationTree appTree)
        {
            if (appTree == null) throw new ArgumentNullException("appTree");

            BaseTree tree;

            var controllerAttempt = appTree.TryGetControllerTree();
            if (controllerAttempt.Success)
            {
                var legacyAtt = controllerAttempt.Result.GetCustomAttribute<LegacyBaseTreeAttribute>(false);
                if (legacyAtt == null)
                {
                    LogHelper.Warn<LegacyTreeDataConverter>("Cannot render tree: " + appTree.Alias + ". Cannot render a " + typeof(TreeController) + " tree type with the legacy web services unless attributed with " + typeof(LegacyBaseTreeAttribute));
                    return null;
                }

                var treeDef = new TreeDefinition(
                    legacyAtt.BaseTreeType,
                    new ApplicationTree(false, true, (byte)appTree.SortOrder, appTree.ApplicationAlias, appTree.Alias, appTree.Title, appTree.IconClosed, appTree.IconOpened, "", legacyAtt.BaseTreeType.GetFullNameWithAssembly(), ""),
                    new Application(appTree.Alias, appTree.Alias, "", 0));

                tree = treeDef.CreateInstance();
                tree.TreeAlias = appTree.Alias;

            }
            else
            {
                //get the tree that we need to render                    
                var treeDef = TreeDefinitionCollection.Instance.FindTree(appTree.Alias);
                if (treeDef == null)
                {
                    return null;
                }
                tree = treeDef.CreateInstance();
            }

            return tree;
        }

        /// <summary>
        /// This is used by any legacy services that require rendering a BaseTree, if a new controller tree is detected it will try to invoke it's legacy predecessor.
        /// </summary>
        /// <param name="appTreeService"></param>
        /// <param name="treeType"></param>
        /// <returns></returns>
        internal static BaseTree GetLegacyTreeForLegacyServices(IApplicationTreeService appTreeService, string treeType)
        {
            if (appTreeService == null) throw new ArgumentNullException("appTreeService");
            if (treeType == null) throw new ArgumentNullException("treeType");

            //first get the app tree definition so we can then figure out if we need to load by legacy or new
            //now we'll look up that tree
            var appTree = appTreeService.GetByAlias(treeType);
            if (appTree == null)
                throw new InvalidOperationException("No tree found with alias " + treeType);

            return GetLegacyTreeForLegacyServices(appTree);
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
                    seperators.Add(collection.Items.Count());
                }
                else
                {
                    var menuItem = collection.Items.Add(t, ui.Text("actions", t.Alias));

                    var currentAction = t;

                    //First try to get a URL/title from the legacy action,
                    // if that doesn't work, try to get the legacy confirm view
                    // if that doesn't work and there's no jsAction in there already then add the legacy js method call
                    Attempt
                        .Try(GetUrlAndTitleFromLegacyAction(currentAction, xmlTreeNode.NodeID, xmlTreeNode.NodeType, xmlTreeNode.Text, currentSection),
                             action => menuItem.LaunchDialogUrl(action.Url, action.DialogTitle))
                        .OnFailure(() => GetLegacyConfirmView(currentAction, currentSection),
                                   view => menuItem.LaunchDialogView(
                                       view,
                                       ui.GetText("defaultdialogs", "confirmdelete") + " '" + xmlTreeNode.Text + "' ?"))
                        .OnFailure(() => menuItem.AdditionalData.ContainsKey(MenuItem.JsActionKey)
                                             ? Attempt.Fail(false)
                                             : Attempt.Succeed(true),
                                   b => menuItem.ExecuteLegacyJs(menuItem.Action.JsFunctionName));
                    
                    numAdded++;
                }
            }
            var length = collection.Items.Count();
            foreach (var s in seperators)
            {
                if (length >= s)
                {
                    collection.Items.ElementAt(s).SeperatorBefore = true;
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
                return Attempt<string>.Fail();
            }

            switch (action.JsFunctionName)
            {
                case "UmbClientMgr.appActions().actionDelete()":
                    return Attempt.Succeed(
                        Core.Configuration.GlobalSettings.Path.EnsureEndsWith('/') + "views/common/dialogs/legacydelete.html");
            }

            return Attempt<string>.Fail();
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
                return Attempt<LegacyUrlAction>.Fail();
            }

            switch (action.JsFunctionName)
            {
                case "UmbClientMgr.appActions().actionNew()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "create.aspx?nodeId=" + nodeId + "&nodeType=" + nodeType + "&nodeName=" + nodeName + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "create")));
                case "UmbClientMgr.appActions().actionNewFolder()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "createFolder.aspx?nodeId=" + nodeId + "&nodeType=" + nodeType + "&nodeName=" + nodeName + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "create")));
                case "UmbClientMgr.appActions().actionSort()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/sort.aspx?id=" + nodeId + "&nodeType=" + nodeType + "&app=" + currentSection + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "sort")));
                case "UmbClientMgr.appActions().actionRights()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/cruds.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "rights")));
                case "UmbClientMgr.appActions().actionProtect()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/protectPage.aspx?mode=cut&nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "protect")));
                case "UmbClientMgr.appActions().actionRollback()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/rollback.aspx?nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "rollback")));
                case "UmbClientMgr.appActions().actionNotify()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/notifications.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "notify")));
                case "UmbClientMgr.appActions().actionPublish()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/publish.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "publish")));
                case "UmbClientMgr.appActions().actionChangeDocType()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/ChangeDocType.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "changeDocType")));
                case "UmbClientMgr.appActions().actionToPublish()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/SendPublish.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "sendtopublish")));
                case "UmbClientMgr.appActions().actionRePublish()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/republish.aspx?rnd=" + DateTime.UtcNow.Ticks,
                            "Republishing entire site"));
                case "UmbClientMgr.appActions().actionAssignDomain()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/assignDomain2.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "assignDomain")));                
                case "UmbClientMgr.appActions().actionSendToTranslate()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/sendToTranslation.aspx?id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "sendToTranslate")));
                case "UmbClientMgr.appActions().actionEmptyTranscan()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/emptyTrashcan.aspx?type=" + currentSection,
                            ui.GetText("actions", "emptyTrashcan")));
                case "UmbClientMgr.appActions().actionImport()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/importDocumentType.aspx",
                            ui.GetText("actions", "importDocumentType")));
                case "UmbClientMgr.appActions().actionExport()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/exportDocumentType.aspx?nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ""));
                case "UmbClientMgr.appActions().actionAudit()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/viewAuditTrail.aspx?nodeId=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "auditTrail")));
                case "UmbClientMgr.appActions().actionMove()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/moveOrCopy.aspx?app=" + currentSection + "&mode=cut&id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "move")));
                case "UmbClientMgr.appActions().actionCopy()":
                    return Attempt.Succeed(
                        new LegacyUrlAction(
                            "dialogs/moveOrCopy.aspx?app=" + currentSection + "&mode=copy&id=" + nodeId + "&rnd=" + DateTime.UtcNow.Ticks,
                            ui.GetText("actions", "copy")));
            }
            return Attempt<LegacyUrlAction>.Fail();
        }

        /// <summary>
        /// Converts a legacy XmlTreeNode to a new TreeNode
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="xmlTreeNode"></param>
        /// <param name="urlHelper"></param>
        /// <param name="currentSection"></param>
        /// <param name="currentQueryStrings">
        /// The current query strings for the request - this is used to append the query strings to the menu URL of the item being rendered since the menu
        /// actually belongs to this same node (request) the query strings need to exist so the menu can be rendered in some cases.
        /// </param>
        /// <param name="isRoot"></param>
        /// <returns></returns>
        internal static TreeNode ConvertFromLegacy(string parentId, XmlTreeNode xmlTreeNode, UrlHelper urlHelper, string currentSection, FormDataCollection currentQueryStrings, bool isRoot = false)
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
            //if there are no menu items, then this will be empty
            var menuSource = "";
            if (xmlTreeNode.Menu != null && xmlTreeNode.Menu.Any())
            {
                menuSource = urlHelper.GetUmbracoApiService<LegacyTreeController>("GetMenu");
                //these are the absolute required query strings
                var menuQueryStrings = new Dictionary<string, object>
                    {
                        {"id", (isRoot ? "-1" : xmlTreeNode.NodeID)},
                        {"treeType", xmlTreeNode.TreeType},
                        {"parentId", (isRoot ? "-1" : parentId)},
                        {"section", currentSection}
                    };
                //append the extra ones on this request
                foreach (var i in currentQueryStrings.Where(x => menuQueryStrings.Keys.Contains(x.Key) == false))
                {
                    menuQueryStrings.Add(i.Key, i.Value);
                }
                
                menuSource = menuSource.AppendQueryStringToUrl(menuQueryStrings.ToQueryString());    
            }
            

            //TODO: Might need to add stuff to additional attributes

            var node = new TreeNode(xmlTreeNode.NodeID, isRoot ? null : parentId, childNodesSource, menuSource)
            {
                HasChildren = xmlTreeNode.HasChildren,
                Icon = xmlTreeNode.Icon,
                Name = xmlTreeNode.Text,
                NodeType = xmlTreeNode.NodeType
            };
            if (isRoot)
            {
                node.AdditionalData.Add("treeAlias", xmlTreeNode.TreeType);
            }

            foreach (var appliedClass in xmlTreeNode.Style.AppliedClasses)
            {
                node.CssClasses.Add(appliedClass);
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

        internal static TreeNodeCollection ConvertFromLegacy(string parentId, XmlTree xmlTree, UrlHelper urlHelper, string currentSection, FormDataCollection currentQueryStrings)
        {
            //TODO: Once we get the editor URL stuff working we'll need to figure out how to convert 
            // that over to use the old school ui.xml stuff for these old trees and however the old menu items worked.

            var collection = new TreeNodeCollection();
            foreach (var x in xmlTree.treeCollection)
            {
                collection.Add(ConvertFromLegacy(parentId, x, urlHelper, currentSection, currentQueryStrings));
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