using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Services;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Used to return tree root nodes
    /// </summary>
    [AngularJsonOnlyConfiguration]
    [PluginController("UmbracoTrees")]
    public class ApplicationTreeController : UmbracoAuthorizedApiController
    {
        private readonly ITreeService _treeService;
        private readonly ISectionService _sectionService;

        public ApplicationTreeController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger,
            IRuntimeState runtimeState, ITreeService treeService, ISectionService sectionService, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _treeService = treeService;
            _sectionService = sectionService;
        }

        /// <summary>
        /// Returns the tree nodes for an application
        /// </summary>
        /// <param name="application">The application to load tree for</param>
        /// <param name="tree">An optional single tree alias, if specified will only load the single tree for the request app</param>
        /// <param name="queryStrings"></param>
        /// <param name="use">Tree use.</param>
        /// <returns></returns>
        public async Task<TreeRootNode> GetApplicationTrees(string application, string tree, [System.Web.Http.ModelBinding.ModelBinder(typeof(HttpQueryStringModelBinder))]FormDataCollection queryStrings, TreeUse use = TreeUse.Main)
        {
            application = application.CleanForXss();

            if (string.IsNullOrEmpty(application))
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var section = _sectionService.GetByAlias(application);
            if (section == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            //find all tree definitions that have the current application alias
            var groupedTrees = _treeService.GetBySectionGrouped(application, use);
            var allTrees = groupedTrees.Values.SelectMany(x => x).ToList();

            if (allTrees.Count == 0)
            {
                //if there are no trees defined for this section but the section is defined then we can have a simple
                //full screen section without trees
                var name = Services.TextService.Localize("sections", application);
                return TreeRootNode.CreateSingleTreeRoot(Constants.System.RootString, null, null, name, TreeNodeCollection.Empty, true);
            }

            // handle request for a specific tree / or when there is only one tree
            if (!tree.IsNullOrWhiteSpace() || allTrees.Count == 1)
            {
                var t = tree.IsNullOrWhiteSpace()
                    ? allTrees[0]
                    : allTrees.FirstOrDefault(x => x.TreeAlias == tree);

                if (t == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                var treeRootNode = await GetTreeRootNode(t, Constants.System.Root, queryStrings);
                if (treeRootNode != null)
                    return treeRootNode;

                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // handle requests for all trees
            // for only 1 group
            if (groupedTrees.Count == 1)
            {
                var nodes = new TreeNodeCollection();
                foreach (var t in allTrees)
                {
                    var node = await TryGetRootNode(t, queryStrings);
                    if (node != null)
                        nodes.Add(node);
                }

                var name = Services.TextService.Localize("sections", application);

                if (nodes.Count > 0)
                {
                    var treeRootNode = TreeRootNode.CreateMultiTreeRoot(nodes);
                    treeRootNode.Name = name;
                    return treeRootNode;
                }

                // otherwise it's a section with all empty trees, aka a fullscreen section
                // todo is this true? what if we just failed to TryGetRootNode on all of them? SD: Yes it's true but we should check the result of TryGetRootNode and throw?
                return TreeRootNode.CreateSingleTreeRoot(Constants.System.RootString, null, null, name, TreeNodeCollection.Empty, true);
            }

            // for many groups
            var treeRootNodes = new List<TreeRootNode>();
            foreach (var (groupName, trees) in groupedTrees)
            {
                var nodes = new TreeNodeCollection();
                foreach (var t in trees)
                {
                    var node = await TryGetRootNode(t, queryStrings);
                    if (node != null)
                        nodes.Add(node);
                }

                if (nodes.Count == 0)
                    continue;

                // no name => third party
                // use localization key treeHeaders/thirdPartyGroup
                // todo this is an odd convention
                var name = groupName.IsNullOrWhiteSpace() ? "thirdPartyGroup" : groupName;

                var groupRootNode = TreeRootNode.CreateGroupNode(nodes, application);
                groupRootNode.Name = Services.TextService.Localize("treeHeaders", name);
                treeRootNodes.Add(groupRootNode);
            }

            return TreeRootNode.CreateGroupedMultiTreeRoot(new TreeNodeCollection(treeRootNodes.OrderBy(x => x.Name)));
        }

        /// <summary>
        /// Tries to get the root node of a tree.
        /// </summary>
        /// <remarks>
        /// <para>Returns null if the root node could not be obtained due to an HttpResponseException,
        /// which probably indicates that the user isn't authorized to view that tree.</para>
        /// </remarks>
        private async Task<TreeNode> TryGetRootNode(Tree tree, FormDataCollection querystring)
        {
            if (tree == null) throw new ArgumentNullException(nameof(tree));

            try
            {
                return await GetRootNode(tree, querystring);
            }
            catch (HttpResponseException)
            {
                // if this occurs its because the user isn't authorized to view that tree,
                // in this case since we are loading multiple trees we will just return
                // null so that it's not added to the list.
                return null;
            }
        }

        /// <summary>
        /// Get the tree root node of a tree.
        /// </summary>
        private async Task<TreeRootNode> GetTreeRootNode(Tree tree, int id, FormDataCollection querystring)
        {
            if (tree == null) throw new ArgumentNullException(nameof(tree));

            var children = await GetChildren(tree, id, querystring);
            var rootNode = await GetRootNode(tree, querystring);

            var sectionRoot = TreeRootNode.CreateSingleTreeRoot(
                Constants.System.RootString,
                rootNode.ChildNodesUrl,
                rootNode.MenuUrl,
                rootNode.Name,
                children,
                tree.IsSingleNodeTree);

            // assign the route path based on the root node, this means it will route there when the
            // section is navigated to and no dashboards will be available for this section
            sectionRoot.RoutePath = rootNode.RoutePath;
            sectionRoot.Path = rootNode.Path;

            foreach (var d in rootNode.AdditionalData)
                sectionRoot.AdditionalData[d.Key] = d.Value;

            return sectionRoot;
        }

        /// <summary>
        /// Gets the root node of a tree.
        /// </summary>
        private async Task<TreeNode> GetRootNode(Tree tree, FormDataCollection querystring)
        {
            if (tree == null) throw new ArgumentNullException(nameof(tree));

            var controller = (TreeControllerBase) await GetApiControllerProxy(tree.TreeControllerType, "GetRootNode", querystring);
            var rootNode = controller.GetRootNode(querystring);
            if (rootNode == null)
                throw new InvalidOperationException($"Failed to get root node for tree \"{tree.TreeAlias}\".");
            return rootNode;
        }

        /// <summary>
        /// Get the child nodes of a tree node.
        /// </summary>
        private async Task<TreeNodeCollection> GetChildren(Tree tree, int id, FormDataCollection querystring)
        {
            if (tree == null) throw new ArgumentNullException(nameof(tree));

            // the method we proxy has an 'id' parameter which is *not* in the querystring,
            // we need to add it for the proxy to work (else, it does not find the method,
            // when trying to run auth filters etc).
            var d = querystring?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();
            d["id"] = null;
            var proxyQuerystring = new FormDataCollection(d);

            var controller = (TreeControllerBase) await GetApiControllerProxy(tree.TreeControllerType, "GetNodes", proxyQuerystring);
            return controller.GetNodes(id.ToInvariantString(), querystring);
        }

        /// <summary>
        /// Gets a proxy to a controller for a specified action.
        /// </summary>
        /// <param name="controllerType">The type of the controller.</param>
        /// <param name="action">The action.</param>
        /// <param name="querystring">The querystring.</param>
        /// <returns>An instance of the controller.</returns>
        /// <remarks>
        /// <para>Creates an instance of the <paramref name="controllerType"/> and initializes it with a route
        /// and context etc. so it can execute the specified <paramref name="action"/>. Runs the authorization
        /// filters for that action, to ensure that the user has permission to execute it.</para>
        /// </remarks>
        private async Task<object> GetApiControllerProxy(Type controllerType, string action, FormDataCollection querystring)
        {
            // note: this is all required in order to execute the auth-filters for the sub request, we
            // need to "trick" web-api into thinking that it is actually executing the proxied controller.

            var context = ControllerContext;

            // get the controller
            var controller = (ApiController) DependencyResolver.Current.GetService(controllerType)
                             ?? throw new Exception($"Failed to create controller of type {controllerType.FullName}.");

            // create the proxy URL for the controller action
            var proxyUrl = context.Request.RequestUri.GetLeftPart(UriPartial.Authority)
                      + context.Request.GetUrlHelper().GetUmbracoApiService(action, controllerType)
                      + "?" + querystring.ToQueryString();

            // create proxy route data specifying the action & controller to execute
            var proxyRoute = new HttpRouteData(
                context.RouteData.Route,
                new HttpRouteValueDictionary(new { action, controller = ControllerExtensions.GetControllerName(controllerType) }));

            // create a proxy request
            var proxyRequest = new HttpRequestMessage(HttpMethod.Get, proxyUrl);

            // create a proxy controller context
            var proxyContext = new HttpControllerContext(context.Configuration, proxyRoute, proxyRequest)
            {
                ControllerDescriptor = new HttpControllerDescriptor(context.ControllerDescriptor.Configuration, ControllerExtensions.GetControllerName(controllerType), controllerType),
                RequestContext = context.RequestContext,
                Controller = controller
            };

            // wire everything
            controller.ControllerContext = proxyContext;
            controller.Request = proxyContext.Request;
            controller.RequestContext.RouteData = proxyRoute;

            // auth
            var authResult = await controller.ControllerContext.InvokeAuthorizationFiltersForRequest();
            if (authResult != null)
                throw new HttpResponseException(authResult);

            return controller;
        }
    }
}
