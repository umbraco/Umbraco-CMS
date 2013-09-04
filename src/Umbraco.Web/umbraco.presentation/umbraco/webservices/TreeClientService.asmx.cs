using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Services;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebServices;
using umbraco.BusinessLogic;
using umbraco.businesslogic;
using umbraco.presentation.umbraco.controls;
using umbraco.cms.presentation.Trees;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.EnterpriseServices;
using System.IO;
using System.Web.UI;
using umbraco.controls.Tree;
using Umbraco.Web.Trees;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Client side ajax utlities for the tree
    /// </summary>
    [ScriptService]
    [WebService]
    public class TreeClientService : UmbracoAuthorizedWebService
    {

        /// <summary>
        /// Returns a key/value object with: json, app, js as the keys
        /// </summary>	
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Dictionary<string, string> GetInitAppTreeData(string app, string treeType, bool showContextMenu, bool isDialog, TreeDialogModes dialogMode, string functionToCall, string nodeKey)
        {
            AuthorizeRequest(app, true);

            var treeCtl = new TreeControl()
            {
                ShowContextMenu = showContextMenu,
                IsDialog = isDialog,
                DialogMode = dialogMode,
                App = app,
                TreeType = string.IsNullOrEmpty(treeType) ? "" : treeType, //don't set the tree type unless explicitly set
                NodeKey = string.IsNullOrEmpty(nodeKey) ? "" : nodeKey,
                StartNodeID = -1, //TODO: set this based on parameters!
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
                BaseTree tree = null;
                var xTree = new XmlTree();
                
                //first get the app tree definition so we can then figure out if we need to load by legacy or new
                //now we'll look up that tree
                var appTree = Services.ApplicationTreeService.GetByAlias(treeType);
                if (appTree == null)
                    throw new InvalidOperationException("No tree found with alias " + treeType);

                var controllerAttempt = appTree.TryGetControllerTree();
                if (controllerAttempt.Success)
                {
                    var legacyAtt = controllerAttempt.Result.GetCustomAttribute<LegacyBaseTreeAttribute>(false);
                    if (legacyAtt == null)
                    {
                        throw new InvalidOperationException("Cannot render a " + typeof (TreeApiController) + " tree type with the legacy web services unless attributed with " + typeof (LegacyBaseTreeAttribute));
                    }

                    var treeDef = new TreeDefinition(
                        legacyAtt.BaseTreeType,
                        new ApplicationTree(false, true, appTree.SortOrder, appTree.ApplicationAlias, appTree.Alias, appTree.Title, appTree.IconClosed, appTree.IconOpened, "", legacyAtt.BaseTreeType.GetFullNameWithAssembly(), ""),
                        new Application(treeType, treeType, "", 0));

                    tree = treeDef.CreateInstance();
                    tree.TreeAlias = appTree.Alias;

                    //var queryStrings = new FormDataCollection(new Dictionary<string, string>
                    //    {
                    //        {TreeQueryStringParameters.Application, app},
                    //        {TreeQueryStringParameters.DialogMode, isDialog.ToString()}
                    //    });

                    //var context = WebApiHelper.CreateContext(new HttpMethod("GET"), Context.Request.Url, new HttpContextWrapper(Context));

                    //var rootAttempt = appTree.TryGetRootNodeFromControllerTree(
                    //    queryStrings,
                    //    context);

                    //if (rootAttempt.Success)
                    //{
                    //    tree = new LegacyBaseTreeWrapper(treeType, app, rootAttempt.Result);
                    //}
                }
                else
                {
                    //get the tree that we need to render
                    
                    var treeDef = TreeDefinitionCollection.Instance.FindTree(treeType);
                    //if (treeDef == null)
                    //{
                    //    // Load all LEGACY Trees by attribute and add them to the XML config
                    //    var legacyTreeTypes = PluginManager.Current.ResolveAttributedTrees();
                    //    var found = legacyTreeTypes
                    //        .Select(x => new { att = x.GetCustomAttribute<businesslogic.TreeAttribute>(false), type = x })
                    //        .FirstOrDefault(x => x.att.Alias == treeType);
                    //    if (found == null)
                    //    {
                    //        throw new InvalidOperationException("The " + GetType() + " service can only return data for legacy tree types");
                    //    }
                    //    treeDef = new TreeDefinition(
                    //        found.type,
                    //        new ApplicationTree(found.att.Silent, found.att.Initialize, (byte)found.att.SortOrder, found.att.ApplicationAlias, found.att.Alias, found.att.Title, found.att.IconClosed, found.att.IconOpen, "", found.type.GetFullNameWithAssembly(), found.att.Action),
                    //        new Application(treeType, treeType, "", 0));

                    //    tree = treeDef.CreateInstance();
                    //}
                    //else
                    //{
                    //    tree = treeDef.CreateInstance();
                    //}

                    tree = treeDef.CreateInstance();
                }

                tree.ShowContextMenu = showContextMenu;
                tree.IsDialog = isDialog;
                tree.DialogMode = dialogMode;
                tree.NodeKey = string.IsNullOrEmpty(nodeKey) ? "" : nodeKey;
                tree.FunctionToCall = string.IsNullOrEmpty(functionToCall) ? "" : functionToCall;
                //this would be nice to set, but no parameters :( 
                //tree.StartNodeID =    

                //now render it's start node
                xTree.Add(tree.RootNode);

                returnVal.Add("json", xTree.ToString());    
            }

            returnVal.Add("app", app);
            returnVal.Add("js", treeCtl.JSCurrApp);

            return returnVal;
        }

        [Obsolete("Use the AuthorizeRequest methods on the base class UmbracoAuthorizedWebService instead")]
        public static void Authorize()
        {
            if (!BasePages.BasePage.ValidateUserContextID(BasePages.BasePage.umbracoUserContextID))
                throw new Exception("Client authorization failed. User is not logged in");
        }

    }
}
