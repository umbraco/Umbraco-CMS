using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Management.Instrumentation;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Core.Composing;
using ApplicationTree = Umbraco.Web.Models.ContentEditing.ApplicationTree;

namespace Umbraco.Web.Trees
{
    public class TreeControllerResolver
    {
        private readonly TreeCollection _trees;
        private readonly UmbracoApiControllerTypeCollection _apiControllers;

        public TreeControllerResolver(TreeCollection trees, UmbracoApiControllerTypeCollection apiControllers)
        {
            _trees = trees;
            _apiControllers = apiControllers;
        }

        private static readonly ConcurrentDictionary<Type, TreeAttribute> TreeAttributeCache = new ConcurrentDictionary<Type, TreeAttribute>();
        private static readonly ConcurrentDictionary<string, Type> ResolvedControllerTypes = new ConcurrentDictionary<string, Type>();

        private TreeAttribute GetTreeAttribute(Type treeControllerType)
        {
            return TreeAttributeCache.GetOrAdd(treeControllerType, type =>
            {
                //Locate the tree attribute
                var treeAttributes = type
                    .GetCustomAttributes<TreeAttribute>(false)
                    .ToArray();

                if (treeAttributes.Length == 0)
                {
                    throw new InvalidOperationException("The Tree controller is missing the " + typeof(TreeAttribute).FullName + " attribute");
                }

                //assign the properties of this object to those of the metadata attribute
                return treeAttributes[0];
            });
        }

        internal TreeAttribute GetTreeAttribute(ApplicationTree tree)
        {
            throw new NotImplementedException();
            //return ResolvedControllerTypes.GetOrAdd(tree.Alias, s =>
            //{
            //    var controllerType = _apiControllers
            //        .OfType<ApplicationTreeController>()
            //        .FirstOrDefault(x => x.)
            //});

            //return GetTreeAttribute(tree.GetRuntimeType());
        }

        private Type GetControllerType(ApplicationTree tree)
        {
            throw new NotImplementedException();
        }
        

        internal Attempt<Type> TryGetControllerTree(ApplicationTree appTree)
        {
            throw new NotImplementedException();

            ////get reference to all TreeApiControllers
            //var controllerTrees = _apiControllers
            //    .Where(TypeHelper.IsTypeAssignableFrom<TreeController>)
            //    .ToArray();

            ////find the one we're looking for
            //var foundControllerTree = controllerTrees.FirstOrDefault(x => x == appTree.GetRuntimeType());
            //if (foundControllerTree == null)
            //{
            //    return Attempt<Type>.Fail(new InstanceNotFoundException("Could not find tree of type " + appTree.Type + " in any loaded DLLs"));
            //}
            //return Attempt.Succeed(foundControllerTree);
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
        internal async Task<Attempt<TreeNode>> TryGetRootNodeFromControllerTree(ApplicationTree appTree, FormDataCollection formCollection, HttpControllerContext controllerContext)
        {
            var foundControllerTreeAttempt = TryGetControllerTree(appTree);
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
                //fixme - will this 'just' work now?
                proxiedControllerContext.RequestContext = controllerContext.RequestContext;

                ////In WebApi2, this is required to be set:
                ////      proxiedControllerContext.RequestContext = controllerContext.RequestContext
                //// but we need to do this with reflection because of codebase changes between version 4/5
                ////NOTE: Use TypeHelper here since the reflection is cached
                //var controllerContextRequestContext = TypeHelper.GetProperty(controllerContext.GetType(), "RequestContext").GetValue(controllerContext);
                //TypeHelper.GetProperty(proxiedControllerContext.GetType(), "RequestContext").SetValue(proxiedControllerContext, controllerContextRequestContext);
            }

            instance.ControllerContext = proxiedControllerContext;
            instance.Request = controllerContext.Request;

            if (WebApiVersionCheck.WebApiVersion >= Version.Parse("5.0.0"))
            {
                
                //fixme - will this 'just' work now?
                instance.RequestContext.RouteData = proxiedRouteData;

                ////now we can change the request context's route data to be the proxied route data - NOTE: we cannot do this directly above
                //// because it will detect that the request context is different throw an exception. This is a change in webapi2 and we need to set
                //// this with reflection due to codebase changes between version 4/5
                ////      instance.RequestContext.RouteData = proxiedRouteData;
                ////NOTE: Use TypeHelper here since the reflection is cached
                //var instanceRequestContext = TypeHelper.GetProperty(typeof(ApiController), "RequestContext").GetValue(instance);
                //TypeHelper.GetProperty(instanceRequestContext.GetType(), "RouteData").SetValue(instanceRequestContext, proxiedRouteData);
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
                ? Attempt<TreeNode>.Fail(new InvalidOperationException("Could not return a root node for tree " + appTree.Alias))
                : Attempt<TreeNode>.Succeed(node);
        }

        internal Attempt<TreeNodeCollection> TryLoadFromControllerTree(ApplicationTree appTree, string id, FormDataCollection formCollection, HttpControllerContext controllerContext)
        {
            var foundControllerTreeAttempt = TryGetControllerTree(appTree);
            if (foundControllerTreeAttempt.Success == false)
                return Attempt<TreeNodeCollection>.Fail(foundControllerTreeAttempt.Exception);

            // instantiate it, since we are proxying, we need to setup the instance with our current context
            var foundControllerTree = foundControllerTreeAttempt.Result;
            var instance = (TreeController) DependencyResolver.Current.GetService(foundControllerTree);
            if (instance == null)
                throw new Exception("Failed to get tree " + foundControllerTree.FullName + ".");

            instance.ControllerContext = controllerContext;
            instance.Request = controllerContext.Request;

            // return its data
            return Attempt.Succeed(instance.GetNodes(id, formCollection));
        }

    }

}
