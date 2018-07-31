using System;
using Umbraco.Core.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using umbraco;
using umbraco.cms.businesslogic;
using umbraco.cms.presentation.Trees;
using umbraco.controls.Tree;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Security;
using Umbraco.Web.WebServices;

namespace umbraco.controls.Tree
{
    /// <summary>
    /// Client side ajax utlities for the tree
    /// </summary>
    [ScriptService]
    [WebService]
    public class CustomTreeService : UmbracoWebService
    {
        /// <summary>
        /// Returns some info about the node such as path and id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public NodeInfo GetNodeInfo(int id)
        {
            Authorize();

            //var node = new CMSNode(id);
            var node = Services.EntityService.Get(id);
            return new NodeInfo()
            {
                Id = node.Id,
                Path = node.Path,
                PathAsNames = string.Join("->",
                   GetPathNames(node.Path.Split(',')
                                       .Select(x => int.Parse(x))
                                       .ToArray()))
            };
        }

        /// <summary>
        /// returns the node names for each id passed in
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private string[] GetPathNames(int[] ids)
        {
            return ids
                .Where(x => x != -1)
                //.Select(x => new CMSNode(x).Text).ToArray();
                .Select(x => Services.EntityService.Get(x).Name).ToArray();
        }

        /// <summary>
        /// Returns a key/value object with: json, app, js as the keys
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Dictionary<string, string> GetInitAppTreeData(string app, string treeType, bool showContextMenu, bool isDialog, TreeDialogModes dialogMode, string functionToCall, string nodeKey)
        {
            Authorize();

            var treeCtl = new TreeControl()
            {
                ShowContextMenu = showContextMenu,
                IsDialog = isDialog,
                DialogMode = dialogMode,
                App = app,
                TreeType = string.IsNullOrEmpty(treeType) ? "" : treeType, //don't set the tree type unless explicitly set
                NodeKey = string.IsNullOrEmpty(nodeKey) ? "" : nodeKey,
                //StartNodeID = -1, //TODO: set this based on parameters!
                FunctionToCall = string.IsNullOrEmpty(functionToCall) ? "" : functionToCall
            };

            var returnVal = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(treeType))
            {
                //if there's not tree type specified, then render out the tree as per normal with the normal
                //way of doing things
                returnVal.Add("json", treeCtl.GetJSONInitNode());
            }
            else
            {
                //since 4.5.1 has a bug in it, it ignores if the treeType is specified and will always only render
                //the whole APP not just a specific tree.
                //this is a work around for this bug until it is fixed (which should be fixed in 4.5.2

                //get the tree that we need to render
                var tree = TreeDefinitionCollection.Instance.FindTree(treeType).CreateInstance();
                tree.ShowContextMenu = showContextMenu;
                tree.IsDialog = isDialog;
                tree.DialogMode = dialogMode;
                tree.NodeKey = string.IsNullOrEmpty(nodeKey) ? "" : nodeKey;
                tree.FunctionToCall = string.IsNullOrEmpty(functionToCall) ? "" : functionToCall;

                //now render it's start node
                var xTree = new XmlTree();

                //we're going to hijack the node name here to make it say content/media
                var node = tree.RootNode;
                if (node.Text.Equals("[FilteredContentTree]")) node.Text = Services.TextService.Localize("content");
                else if (node.Text.Equals("[FilteredMediaTree]")) node.Text = Services.TextService.Localize("media");
                xTree.Add(node);

                returnVal.Add("json", xTree.ToString());
            }

            returnVal.Add("app", app);
            returnVal.Add("js", treeCtl.JSCurrApp);

            return returnVal;
        }

        internal void Authorize()
        {
            if (ValidateCurrentUser() == false)
                throw new Exception("Client authorization failed. User is not logged in");
        }


        /// <summary>
        /// Validates the currently logged in user and ensures they are not timed out
        /// </summary>
        /// <returns></returns>
        private bool ValidateCurrentUser()
        {
            var identity = Context.GetCurrentIdentity(
                //DO NOT AUTO-AUTH UNLESS THE CURRENT HANDLER IS WEBFORMS!
                // Without this check, anything that is using this legacy API, like ui.Text will
                // automatically log the back office user in even if it is a front-end request (if there is
                // a back office user logged in. This can cause problems becaues the identity is changing mid
                // request. For example: http://issues.umbraco.org/issue/U4-4010
                HttpContext.Current.CurrentHandler is Page);

            if (identity != null)
            {
                return true;
            }
            return false;
        }
    }
}
