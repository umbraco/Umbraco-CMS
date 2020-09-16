using System;
using Umbraco.Core;
using System.Collections.Specialized;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Provides a base class for front-end add-in controllers.
    /// </summary>
    [MergeModelStateToChildAction]
    [MergeParentContextViewData]
    public abstract class SurfaceController : PluginController
    {
        protected SurfaceController()
        { }

        protected SurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, ILogger logger, IProfilingLogger profilingLogger, UmbracoHelper umbracoHelper)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, profilingLogger, umbracoHelper)
        { }

        /// <summary>
        /// Redirects to the Umbraco page with the given id
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(int pageId)
        {
            return new RedirectToUmbracoPageResult(pageId,  Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id and passes provided querystring
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryStringValues"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(int pageId, NameValueCollection queryStringValues)
        {
            return new RedirectToUmbracoPageResult(pageId, queryStringValues,  Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id and passes provided querystring
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(int pageId, string queryString)
        {
            return new RedirectToUmbracoPageResult(pageId, queryString,  Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(Guid key)
        {
            return new RedirectToUmbracoPageResult(key, Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id and passes provided querystring
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryStringValues"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(Guid key, NameValueCollection queryStringValues)
        {
            return new RedirectToUmbracoPageResult(key, queryStringValues, Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id and passes provided querystring
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(Guid key, string queryString)
        {
            return new RedirectToUmbracoPageResult(key, queryString, Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IPublishedContent publishedContent)
        {
            return new RedirectToUmbracoPageResult(publishedContent,  Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id and passes provided querystring
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryStringValues"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IPublishedContent publishedContent, NameValueCollection queryStringValues)
        {
            return new RedirectToUmbracoPageResult(publishedContent, queryStringValues,  Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id and passes provided querystring
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IPublishedContent publishedContent, string queryString)
        {
            return new RedirectToUmbracoPageResult(publishedContent, queryString,  Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the currently rendered Umbraco page
        /// </summary>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage()
        {
            return new RedirectToUmbracoPageResult(CurrentPage,  Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the currently rendered Umbraco page and passes provided querystring
        /// </summary>
        /// <param name="queryStringValues"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage(NameValueCollection queryStringValues)
        {
            return new RedirectToUmbracoPageResult(CurrentPage, queryStringValues,  Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the currently rendered Umbraco page and passes provided querystring
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage(string queryString)
        {
            return new RedirectToUmbracoPageResult(CurrentPage, queryString,  Current.UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the currently rendered Umbraco URL
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// this is useful if you need to redirect
        /// to the current page but the current page is actually a rewritten URL normally done with something like
        /// Server.Transfer.
        /// </remarks>
        protected RedirectToUmbracoUrlResult RedirectToCurrentUmbracoUrl()
        {
            return new RedirectToUmbracoUrlResult(UmbracoContext);
        }

        /// <summary>
        /// Returns the currently rendered Umbraco page
        /// </summary>
        /// <returns></returns>
        protected UmbracoPageResult CurrentUmbracoPage()
        {
            return new UmbracoPageResult(ProfilingLogger);
        }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        protected virtual IPublishedContent CurrentPage
        {
            get
            {
                var routeDefAttempt = TryGetRouteDefinitionFromAncestorViewContexts();
                if (routeDefAttempt.Success == false)
                    throw routeDefAttempt.Exception;

                var routeDef = routeDefAttempt.Result;
                return routeDef.PublishedRequest.PublishedContent;
            }
        }

        /// <summary>
        /// we need to recursively find the route definition based on the parent view context
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We may have Child Actions within Child actions so we need to recursively look this up.
        /// see: http://issues.umbraco.org/issue/U4-1844
        /// </remarks>
        private Attempt<RouteDefinition> TryGetRouteDefinitionFromAncestorViewContexts()
        {
            var currentContext = ControllerContext;
            while (currentContext != null)
            {
                var currentRouteData = currentContext.RouteData;
                if (currentRouteData.DataTokens.ContainsKey(Core.Constants.Web.UmbracoRouteDefinitionDataToken))
                    return Attempt.Succeed((RouteDefinition)currentRouteData.DataTokens[Core.Constants.Web.UmbracoRouteDefinitionDataToken]);

                currentContext = currentContext.IsChildAction
                    ? currentContext.ParentActionViewContext
                    : null;
            }
            return Attempt<RouteDefinition>.Fail(
                new InvalidOperationException("Cannot find the Umbraco route definition in the route values, the request must be made in the context of an Umbraco request"));
        }


    }
}
