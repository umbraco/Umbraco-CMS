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
            var appTrees = Services.ApplicationTreeService.GetApplicationTrees(treeParams.Application, true);
            foreach (var appTree in appTrees)
            {
                var tree = LegacyTreeDataConverter.GetLegacyTreeForLegacyServices(Services.ApplicationTreeService, appTree.Alias);
                if (tree != null)
                {
                    tree.SetTreeParameters(treeParams);
                    _xTree.Add(tree.RootNode);
                }
            }
        }

        /// <summary>
        /// This will load the particular ITree object and call it's render method to get the nodes that need to be rendered.
        /// </summary>
        /// <param name="treeParams"></param>
        /// <param name="httpContext"></param>
        private void LoadTree(TreeRequestParams treeParams, HttpContext httpContext)
        {
            var tree = LegacyTreeDataConverter.GetLegacyTreeForLegacyServices(Services.ApplicationTreeService, treeParams.TreeType);
            if (tree != null)
            {
                tree.SetTreeParameters(treeParams);
                tree.Render(ref _xTree);
            }
            else
            {
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
