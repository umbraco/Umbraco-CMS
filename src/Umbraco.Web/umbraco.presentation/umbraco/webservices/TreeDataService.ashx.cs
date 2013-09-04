using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Services;
using Umbraco.Core;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebServices;
using umbraco.BusinessLogic;
using umbraco.cms.presentation;
using umbraco.cms.presentation.Trees;
using System.Threading;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for TreeDataService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class TreeDataService : UmbracoAuthorizedHttpHandler
    {

        public override void ProcessRequest(HttpContext context)
        {
            AuthorizeRequest(true);
            context.Response.ContentType = "application/json";
            context.Response.Write(GetXmlTree(context).ToString());

        }

        public override bool IsReusable
        {
            get
            {
                return false;
            }
        }

        [Obsolete("Use the base class AuthorizeRequest methods in UmbracoAuthorizedHttpHandler")]
        public static void Authorize()
        {
            if (!BasePages.BasePage.ValidateUserContextID(BasePages.BasePage.umbracoUserContextID))
                throw new Exception("Client authorization failed. User is not logged in");

        }

        /// <summary>
        /// Returns the an XmlTree based on the current http request
        /// </summary>
        /// <returns></returns>
        public XmlTree GetXmlTree(HttpContext context)
        {
            var treeParams = TreeRequestParams.FromQueryStrings();

            //validate the current user for the request app!
            AuthorizeRequest(treeParams.Application, true);

            if (string.IsNullOrEmpty(treeParams.TreeType))
                if (!string.IsNullOrEmpty(treeParams.Application))
                    LoadAppTrees(treeParams, context);
                else
                    return new XmlTree(); //returns an empty tree
            else
                LoadTree(treeParams, context);

            return _xTree;
        }

        private XmlTree _xTree = new XmlTree();

        /// <summary>
        /// If the application supports multiple trees, then this function iterates over all of the trees assigned to it
        /// and creates their top level nodes and context menus.
        /// </summary>
        /// <param name="treeParams"></param>
        /// <param name="context"></param>
        private void LoadAppTrees(TreeRequestParams treeParams, HttpContext context)
        {
            //find all tree definitions that have the current application alias
            List<TreeDefinition> treeDefs = TreeDefinitionCollection.Instance.FindActiveTrees(treeParams.Application);

            foreach (TreeDefinition treeDef in treeDefs)
            {
                BaseTree bTree = treeDef.CreateInstance();
                bTree.SetTreeParameters(treeParams);
                _xTree.Add(bTree.RootNode);
            }
        }

        /// <summary>
        /// This will load the particular ITree object and call it's render method to get the nodes that need to be rendered.
        /// </summary>
        /// <param name="treeParams"></param>
        /// <param name="httpContext"></param>
        private void LoadTree(TreeRequestParams treeParams, HttpContext httpContext)
        {

            var appTree = Services.ApplicationTreeService.GetByAlias(treeParams.TreeType);
            if (appTree == null)
                throw new InvalidOperationException("No tree found with alias " + treeParams.TreeType);

            var controllerAttempt = appTree.TryGetControllerTree();
            if (controllerAttempt.Success)
            {
                var legacyAtt = controllerAttempt.Result.GetCustomAttribute<LegacyBaseTreeAttribute>(false);
                if (legacyAtt == null)
                {
                    throw new InvalidOperationException("Cannot render a " + typeof(TreeApiController) + " tree type with the legacy web services unless attributed with " + typeof(LegacyBaseTreeAttribute));
                }

                var treeDef = new TreeDefinition(
                    legacyAtt.BaseTreeType,
                    new ApplicationTree(false, true, appTree.SortOrder, appTree.ApplicationAlias, appTree.Alias, appTree.Title, appTree.IconClosed, appTree.IconOpened, "", legacyAtt.BaseTreeType.GetFullNameWithAssembly(), ""),
                    new Application(treeParams.TreeType, treeParams.TreeType, "", 0));

                var tree = treeDef.CreateInstance();
                tree.TreeAlias = appTree.Alias;
                tree.SetTreeParameters(treeParams);
                tree.Render(ref _xTree);

                //var context = WebApiHelper.CreateContext(new HttpMethod("GET"), httpContext.Request.Url, new HttpContextWrapper(httpContext));

                //var rootAttempt = appTree.TryGetRootNodeFromControllerTree(
                //    LegacyTreeDataConverter.ConvertFromLegacyTreeParams(treeParams),
                //    context);

                //var nodesAttempt = appTree.TryLoadFromControllerTree(
                //    treeParams.StartNodeID.ToInvariantString(),
                //    LegacyTreeDataConverter.ConvertFromLegacyTreeParams(treeParams),
                //    context);

                //if (rootAttempt.Success && nodesAttempt.Success)
                //{
                //    var tree = new LegacyBaseTreeWrapper(treeParams.TreeType, treeParams.Application, rootAttempt.Result, nodesAttempt.Result);
                //    tree.SetTreeParameters(treeParams);
                //    tree.Render(ref _xTree);
                //}
            }
            else
            {
                var treeDef = TreeDefinitionCollection.Instance.FindTree(treeParams.TreeType);

                if (treeDef != null)
                {
                    var bTree = treeDef.CreateInstance();
                    bTree.SetTreeParameters(treeParams);
                    bTree.Render(ref _xTree);
                }
                else
                    LoadNullTree(treeParams);
            }



        }

        /// <summary>
        /// Load an empty tree structure to show the end user that there was a problem loading the tree.
        /// </summary>
        private void LoadNullTree(TreeRequestParams treeParams)
        {
            BaseTree nullTree = new NullTree(treeParams.Application);
            nullTree.SetTreeParameters(treeParams);
            nullTree.Render(ref _xTree);
        }
    }
}
