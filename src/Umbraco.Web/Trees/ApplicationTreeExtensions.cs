using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Instrumentation;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using umbraco.BusinessLogic;
using umbraco.cms.presentation.Trees;
using ApplicationTree = Umbraco.Core.Models.ApplicationTree;
using IAuthorizationFilter = System.Web.Http.Filters.IAuthorizationFilter;
using UrlHelper = System.Web.Http.Routing.UrlHelper;

namespace Umbraco.Web.Trees
{
    internal static class ApplicationTreeExtensions
    {

        internal static Attempt<Type> TryGetControllerTree(this ApplicationTree appTree)
        {
            //get reference to all TreeApiControllers
            var controllerTrees = UmbracoApiControllerResolver.Current.RegisteredUmbracoApiControllers
                                                              .Where(TypeHelper.IsTypeAssignableFrom<TreeController>)
                                                              .ToArray();

            //find the one we're looking for
            var foundControllerTree = controllerTrees.FirstOrDefault(x => x == appTree.GetRuntimeType());
            if (foundControllerTree == null)
            {
                return Attempt<Type>.Fail(new InstanceNotFoundException("Could not find tree of type " + appTree.Type + " in any loaded DLLs"));
            }
            return Attempt.Succeed(foundControllerTree);
        }

        /// <summary>
        /// This will go and get the root node from a controller tree by executing the tree's GetRootNode method
        /// </summary>
        /// <param name="appTree"></param>
        /// <param name="formCollection"></param>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that authorization filters are applied to the sub request 
        /// </remarks>
        internal static async Task<Attempt<TreeNode>> TryGetRootNodeFromControllerTree(this ApplicationTree appTree, FormDataCollection formCollection, HttpControllerContext controllerContext)
        {
            var foundControllerTreeAttempt = appTree.TryGetControllerTree();
            if (foundControllerTreeAttempt.Success == false)
            {
                return Attempt<TreeNode>.Fail(foundControllerTreeAttempt.Exception);
            }
            
            var foundControllerTree = foundControllerTreeAttempt.Result;
            //instantiate it, since we are proxying, we need to setup the instance with our current context
            var instance = (TreeController)DependencyResolver.Current.GetService(foundControllerTree);

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
                new HttpRouteValueDictionary(new {action = "GetRootNode", controller = ControllerExtensions.GetControllerName(instance.GetType())}));

            //create a proxied controller context
            var proxiedControllerContext = new HttpControllerContext(
                controllerContext.Configuration,
                proxiedRouteData,
                new HttpRequestMessage(HttpMethod.Get, proxiedUrl))
                {
                    ControllerDescriptor = new HttpControllerDescriptor(controllerContext.ControllerDescriptor.Configuration, ControllerExtensions.GetControllerName(instance.GetType()), instance.GetType())
                };

            if (WebApiVersionCheck.WebApiVersion >= Version.Parse("5.0.0"))
            {
                //In WebApi2, this is required to be set: 
                //      proxiedControllerContext.RequestContext = controllerContext.RequestContext
                // but we need to do this with reflection because of codebase changes between version 4/5
                //NOTE: Use TypeHelper here since the reflection is cached
                var controllerContextRequestContext = TypeHelper.GetProperty(controllerContext.GetType(), "RequestContext").GetValue(controllerContext);
                TypeHelper.GetProperty(proxiedControllerContext.GetType(), "RequestContext").SetValue(proxiedControllerContext, controllerContextRequestContext);                
            }

            instance.ControllerContext = proxiedControllerContext;
            instance.Request = controllerContext.Request;

            if (WebApiVersionCheck.WebApiVersion >= Version.Parse("5.0.0"))
            {
                //now we can change the request context's route data to be the proxied route data - NOTE: we cannot do this directly above
                // because it will detect that the request context is different throw an exception. This is a change in webapi2 and we need to set
                // this with reflection due to codebase changes between version 4/5
                //      instance.RequestContext.RouteData = proxiedRouteData;
                //NOTE: Use TypeHelper here since the reflection is cached
                var instanceRequestContext = TypeHelper.GetProperty(typeof(ApiController), "RequestContext").GetValue(instance);
                TypeHelper.GetProperty(instanceRequestContext.GetType(), "RouteData").SetValue(instanceRequestContext, proxiedRouteData);
            }

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
                ? Attempt<TreeNode>.Fail(new InvalidOperationException("Could not return a root node for tree " + appTree.Type)) 
                : Attempt<TreeNode>.Succeed(node);
        }

        internal static  Attempt<TreeNodeCollection> TryLoadFromControllerTree(this ApplicationTree appTree, string id, FormDataCollection formCollection, HttpControllerContext controllerContext)
        {
            var foundControllerTreeAttempt = appTree.TryGetControllerTree();
            if (foundControllerTreeAttempt.Success == false)
            {
                return Attempt<TreeNodeCollection>.Fail(foundControllerTreeAttempt.Exception);
            }
            var foundControllerTree = foundControllerTreeAttempt.Result;

            //instantiate it, since we are proxying, we need to setup the instance with our current context
            var instance = (TreeController)DependencyResolver.Current.GetService(foundControllerTree);
            instance.ControllerContext = controllerContext;
            instance.Request = controllerContext.Request;
            //return it's data
            return Attempt.Succeed(instance.GetNodes(id, formCollection));
        }

        internal static Attempt<TreeNode> TryGetRootNodeFromLegacyTree(this ApplicationTree appTree, FormDataCollection formCollection, UrlHelper urlHelper, string currentSection)
        {
            var xmlTreeNodeAttempt = TryGetRootXmlNodeFromLegacyTree(appTree, formCollection, urlHelper);
            if (xmlTreeNodeAttempt.Success == false)
            {
                return Attempt<TreeNode>.Fail(xmlTreeNodeAttempt.Exception);
            }

            //the root can potentially be null, in that case we'll just return a null success which means it won't be included
            if (xmlTreeNodeAttempt.Result == null)
            {
                return Attempt<TreeNode>.Succeed(null);
            }

            var legacyController = new LegacyTreeController(xmlTreeNodeAttempt.Result, appTree.Alias, currentSection, urlHelper);
            var newRoot = legacyController.GetRootNode(formCollection);

            return Attempt.Succeed(newRoot);
            
        }

        internal static Attempt<XmlTreeNode> TryGetRootXmlNodeFromLegacyTree(this ApplicationTree appTree, FormDataCollection formCollection, UrlHelper urlHelper)
        {
            var treeDefAttempt = appTree.TryGetLegacyTreeDef();
            if (treeDefAttempt.Success == false)
            {
                return Attempt<XmlTreeNode>.Fail(treeDefAttempt.Exception);
            }
            var treeDef = treeDefAttempt.Result;
            var bTree = treeDef.CreateInstance();
            var treeParams = new LegacyTreeParams(formCollection);
            bTree.SetTreeParameters(treeParams);

            var xmlRoot = bTree.RootNode;
            
            return Attempt.Succeed(xmlRoot);
        }

        internal static Attempt<TreeDefinition> TryGetLegacyTreeDef(this ApplicationTree appTree)
        {
            //This is how the legacy trees worked....
            var treeDef = TreeDefinitionCollection.Instance.FindTree(appTree.Alias);
            return treeDef == null 
                ? Attempt<TreeDefinition>.Fail(new InstanceNotFoundException("Could not find tree of type " + appTree.Alias)) 
                : Attempt<TreeDefinition>.Succeed(treeDef);
        }

        internal static Attempt<TreeNodeCollection> TryLoadFromLegacyTree(this ApplicationTree appTree, string id, FormDataCollection formCollection, UrlHelper urlHelper, string currentSection)
        {
            var xTreeAttempt = appTree.TryGetXmlTree(id, formCollection);
            if (xTreeAttempt.Success == false)
            {
                return Attempt<TreeNodeCollection>.Fail(xTreeAttempt.Exception);
            }
            return Attempt.Succeed(LegacyTreeDataConverter.ConvertFromLegacy(id, xTreeAttempt.Result, urlHelper, currentSection, formCollection));
        }

        internal static Attempt<MenuItemCollection> TryGetMenuFromLegacyTreeRootNode(this ApplicationTree appTree, FormDataCollection formCollection, UrlHelper urlHelper)
        {
            var rootAttempt = appTree.TryGetRootXmlNodeFromLegacyTree(formCollection, urlHelper);
            if (rootAttempt.Success == false)
            {
                return Attempt<MenuItemCollection>.Fail(rootAttempt.Exception);
            }

            var currentSection = formCollection.GetRequiredString("section");

            var result = LegacyTreeDataConverter.ConvertFromLegacyMenu(rootAttempt.Result, currentSection);            
            return Attempt.Succeed(result);
        }

        internal static Attempt<MenuItemCollection> TryGetMenuFromLegacyTreeNode(this ApplicationTree appTree, string parentId, string nodeId, FormDataCollection formCollection, UrlHelper urlHelper)
        {
            var xTreeAttempt = appTree.TryGetXmlTree(parentId, formCollection);
            if (xTreeAttempt.Success == false)
            {
                return Attempt<MenuItemCollection>.Fail(xTreeAttempt.Exception);
            }

            var currentSection = formCollection.GetRequiredString("section");

            var result = LegacyTreeDataConverter.ConvertFromLegacyMenu(nodeId, xTreeAttempt.Result, currentSection);
            if (result == null)
            {
                return Attempt<MenuItemCollection>.Fail(new ApplicationException("Could not find the node with id " + nodeId + " in the collection of nodes contained with parent id " + parentId));
            }
            return Attempt.Succeed(result);
        }

        private static Attempt<XmlTree> TryGetXmlTree(this ApplicationTree appTree, string id, FormDataCollection formCollection)
        {
            var treeDefAttempt = appTree.TryGetLegacyTreeDef();
            if (treeDefAttempt.Success == false)
            {
                return Attempt<XmlTree>.Fail(treeDefAttempt.Exception);
            }
            var treeDef = treeDefAttempt.Result;
            //This is how the legacy trees worked....
            var bTree = treeDef.CreateInstance();
            var treeParams = new LegacyTreeParams(formCollection);

            //we currently only support an integer id or a string id, we'll refactor how this works
            //later but we'll get this working first
            int startId;
            if (int.TryParse(id, out startId))
            {
                treeParams.StartNodeID = startId;
            }
            else
            {
                treeParams.NodeKey = id;
            }
            var xTree = new XmlTree();
            bTree.SetTreeParameters(treeParams);
            bTree.Render(ref xTree);
            return Attempt.Succeed(xTree);
        }

    }
    
}
