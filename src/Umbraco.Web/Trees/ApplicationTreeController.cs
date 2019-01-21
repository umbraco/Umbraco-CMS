using System;
using System.Collections.Generic;
using System.Globalization;
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
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
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

        public ApplicationTreeController(IGlobalSettings globalSettings, UmbracoContext umbracoContext,
            ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger,
            IRuntimeState runtimeState, ITreeService treeService)
            : base(globalSettings, umbracoContext, sqlContext, services, appCaches, logger, runtimeState)
        {
            _treeService = treeService;
        }

        /// <summary>
        /// Returns the tree nodes for an application
        /// </summary>
        /// <param name="application">The application to load tree for</param>
        /// <param name="tree">An optional single tree alias, if specified will only load the single tree for the request app</param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        [HttpQueryStringFilter("queryStrings")]
        public async Task<TreeRootNode> GetApplicationTrees(string application, string tree, FormDataCollection queryStrings)
        {
            application = application.CleanForXss();

            if (string.IsNullOrEmpty(application)) throw new HttpResponseException(HttpStatusCode.NotFound);

            //find all tree definitions that have the current application alias
            var groupedTrees = _treeService.GetGroupedTrees(application);
            var allTrees = groupedTrees.Values.SelectMany(x => x).ToList();

            if (string.IsNullOrEmpty(tree) == false || allTrees.Count == 1)
            {
                var apptree = !tree.IsNullOrWhiteSpace()
                    ? allTrees.FirstOrDefault(x => x.TreeAlias == tree)
                    : allTrees.FirstOrDefault();

                if (apptree == null) throw new HttpResponseException(HttpStatusCode.NotFound);

                var result = await GetRootForSingleAppTree(
                    apptree,
                    Constants.System.Root.ToString(CultureInfo.InvariantCulture),
                    queryStrings,
                    application);

                //this will be null if it cannot convert to a single root section
                if (result != null)
                {
                    return result;
                }
            }

            //Don't apply fancy grouping logic futher down, if we only have one group of items
            var hasGroups = groupedTrees.Count > 1;
            if (!hasGroups)
            {
                var collection = new TreeNodeCollection();
                foreach (var apptree in allTrees)
                {
                    //return the root nodes for each tree in the app
                    var rootNode = await GetRootForMultipleAppTree(apptree, queryStrings);
                    //This could be null if the tree decides not to return it's root (i.e. the member type tree does this when not in umbraco membership mode)
                    if (rootNode != null)
                    {
                        collection.Add(rootNode);
                    }
                }

                if(collection.Count > 0)
                {
                    var multiTree = TreeRootNode.CreateMultiTreeRoot(collection);
                    multiTree.Name = Services.TextService.Localize("sections/" + application);

                    return multiTree;
                }

                //Otherwise its a application/section with no trees (aka a full screen app)
                //For example we do not have a Forms tree definied in C# & can not attribute with [Tree(isSingleNodeTree:true0]
                var rootId = Constants.System.Root.ToString(CultureInfo.InvariantCulture);
                var section = Services.TextService.Localize("sections/" + application);

                return TreeRootNode.CreateSingleTreeRoot(rootId, null, null, section, TreeNodeCollection.Empty, true);
            }

            var rootNodeGroups = new List<TreeRootNode>();

            //Group trees by [CoreTree] attribute with a TreeGroup property
            foreach (var treeSectionGroup in groupedTrees)
            {
                var treeGroupName = treeSectionGroup.Key;

                var groupNodeCollection = new TreeNodeCollection();
                foreach (var appTree in treeSectionGroup.Value)
                {
                    var rootNode = await GetRootForMultipleAppTree(appTree, queryStrings);
                    if (rootNode != null)
                    {
                        //Add to a new list/collection
                        groupNodeCollection.Add(rootNode);
                    }
                }

                //If treeGroupName == null then its third party
                if (treeGroupName.IsNullOrWhiteSpace())
                {
                    //This is used for the localisation key
                    //treeHeaders/thirdPartyGroup
                    treeGroupName = "thirdPartyGroup";
                }

                if (groupNodeCollection.Count > 0)
                {
                    var groupRoot = TreeRootNode.CreateGroupNode(groupNodeCollection, application);
                    groupRoot.Name = Services.TextService.Localize("treeHeaders/" + treeGroupName);

                    rootNodeGroups.Add(groupRoot);
                }
            }

            return TreeRootNode.CreateGroupedMultiTreeRoot(new TreeNodeCollection(rootNodeGroups.OrderBy(x => x.Name)));
        }

        /// <summary>
        /// Get the root node for an application with multiple trees
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        private async Task<TreeNode> GetRootForMultipleAppTree(Tree tree, FormDataCollection queryStrings)
        {
            if (tree == null) throw new ArgumentNullException(nameof(tree));
            try
            {
                var byControllerAttempt = await TryGetRootNodeFromControllerTree(tree, queryStrings, ControllerContext);
                if (byControllerAttempt.Success)
                {
                    return byControllerAttempt.Result;
                }
            }
            catch (HttpResponseException)
            {
                //if this occurs its because the user isn't authorized to view that tree, in this case since we are loading multiple trees we
                //will just return null so that it's not added to the list.
                return null;
            }

            throw new ApplicationException("Could not get root node for tree type " + tree.TreeAlias);
        }

        /// <summary>
        /// Get the root node for an application with one tree
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        private async Task<TreeRootNode> GetRootForSingleAppTree(Tree tree, string id, FormDataCollection queryStrings, string application)
        {
            var rootId = Constants.System.Root.ToString(CultureInfo.InvariantCulture);
            if (tree == null) throw new ArgumentNullException(nameof(tree));
            var byControllerAttempt = TryLoadFromControllerTree(tree, id, queryStrings, ControllerContext);
            if (!byControllerAttempt.Success)
                throw new ApplicationException("Could not render a tree for type " + tree.TreeAlias);

            var rootNode = await TryGetRootNodeFromControllerTree(tree, queryStrings, ControllerContext);
            if (rootNode.Success == false)
            {
                //This should really never happen if we've successfully got the children above.
                throw new InvalidOperationException("Could not create root node for tree " + tree.TreeAlias);
            }

            var sectionRoot = TreeRootNode.CreateSingleTreeRoot(
                rootId,
                rootNode.Result.ChildNodesUrl,
                rootNode.Result.MenuUrl,
                rootNode.Result.Name,
                byControllerAttempt.Result,
                tree.IsSingleNodeTree);

            //assign the route path based on the root node, this means it will route there when the section is navigated to
            //and no dashboards will be available for this section
            sectionRoot.RoutePath = rootNode.Result.RoutePath;
            sectionRoot.Path = rootNode.Result.Path;

            foreach (var d in rootNode.Result.AdditionalData)
            {
                sectionRoot.AdditionalData[d.Key] = d.Value;
            }
            return sectionRoot;

        }

        /// <summary>
        /// Proxies a request to the destination tree controller to get it's root tree node
        /// </summary>
        /// <param name="appTree"></param>
        /// <param name="formCollection"></param>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that authorization filters are applied to the sub request
        /// </remarks>
        private async Task<Attempt<TreeNode>> TryGetRootNodeFromControllerTree(Tree appTree, FormDataCollection formCollection, HttpControllerContext controllerContext)
        {
            //instantiate it, since we are proxying, we need to setup the instance with our current context
            var instance = (TreeController)DependencyResolver.Current.GetService(appTree.TreeControllerType);

            //NOTE: This is all required in order to execute the auth-filters for the sub request, we
            // need to "trick" web-api into thinking that it is actually executing the proxied controller.

            var urlHelper = controllerContext.Request.GetUrlHelper();
            //create the proxied URL for the controller action
            var proxiedUrl = controllerContext.Request.RequestUri.GetLeftPart(UriPartial.Authority) +
                urlHelper.GetUmbracoApiService("GetRootNode", instance.GetType());
            //add the query strings to it
            proxiedUrl += "?" + formCollection.ToQueryString();
            //create proxy route data specifying the action / controller to execute
            var proxiedRouteData = new HttpRouteData(
                controllerContext.RouteData.Route,
                new HttpRouteValueDictionary(new { action = "GetRootNode", controller = ControllerExtensions.GetControllerName(instance.GetType()) }));

            //create a proxied controller context
            var proxiedControllerContext = new HttpControllerContext(
                controllerContext.Configuration,
                proxiedRouteData,
                new HttpRequestMessage(HttpMethod.Get, proxiedUrl))
            {
                ControllerDescriptor = new HttpControllerDescriptor(controllerContext.ControllerDescriptor.Configuration, ControllerExtensions.GetControllerName(instance.GetType()), instance.GetType()),
                RequestContext = controllerContext.RequestContext
            };

            instance.ControllerContext = proxiedControllerContext;
            instance.Request = controllerContext.Request;
            instance.RequestContext.RouteData = proxiedRouteData;

            //invoke auth filters for this sub request
            var result = await instance.ControllerContext.InvokeAuthorizationFiltersForRequest();
            //if a result is returned it means they are unauthorized, just throw the response.
            if (result != null)
            {
                throw new HttpResponseException(result);
            }

            //return the root
            var node = instance.GetRootNode(formCollection);
            return node == null
                ? Attempt<TreeNode>.Fail(new InvalidOperationException("Could not return a root node for tree " + appTree.TreeAlias))
                : Attempt<TreeNode>.Succeed(node);
        }

        /// <summary>
        /// Proxies a request to the destination tree controller to get it's tree node collection
        /// </summary>
        /// <param name="appTree"></param>
        /// <param name="id"></param>
        /// <param name="formCollection"></param>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        private Attempt<TreeNodeCollection> TryLoadFromControllerTree(Tree appTree, string id, FormDataCollection formCollection, HttpControllerContext controllerContext)
        {
            // instantiate it, since we are proxying, we need to setup the instance with our current context
            var instance = (TreeController)DependencyResolver.Current.GetService(appTree.TreeControllerType);
            if (instance == null)
                throw new Exception("Failed to create tree " + appTree.TreeControllerType + ".");

            //TODO: Shouldn't we be applying the same proxying logic as above so that filters work? seems like an oversight

            instance.ControllerContext = controllerContext;
            instance.Request = controllerContext.Request;

            // return its data
            return Attempt.Succeed(instance.GetNodes(id, formCollection));
        }

    }
}
