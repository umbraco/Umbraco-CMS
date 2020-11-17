using System;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Routing;
using Umbraco.Web.Website.ActionResults;

namespace Umbraco.Web.Website.Controllers
{
    /// <summary>
    /// Provides a base class for front-end add-in controllers.
    /// </summary>
    // TODO: Migrate MergeModelStateToChildAction and MergeParentContextViewData action filters
    // [MergeModelStateToChildAction]
    // [MergeParentContextViewData]
    public abstract class SurfaceController : PluginController
    {
        private readonly IPublishedUrlProvider _publishedUrlProvider;

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

        protected SurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger)
        {
            _publishedUrlProvider = publishedUrlProvider;
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id
        /// </summary>
        /// <param name="contentKey"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(Guid contentKey)
        {
            return new RedirectToUmbracoPageResult(contentKey, _publishedUrlProvider, UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id and passes provided querystring
        /// </summary>
        /// <param name="contentKey"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(Guid contentKey, QueryString queryString)
        {
            return new RedirectToUmbracoPageResult(contentKey, queryString, _publishedUrlProvider, UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given published content
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IPublishedContent publishedContent)
        {
            return new RedirectToUmbracoPageResult(publishedContent, _publishedUrlProvider, UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given published content and passes provided querystring
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IPublishedContent publishedContent, QueryString queryString)
        {
            return new RedirectToUmbracoPageResult(publishedContent, queryString, _publishedUrlProvider, UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the currently rendered Umbraco page
        /// </summary>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage()
        {
            return new RedirectToUmbracoPageResult(CurrentPage, _publishedUrlProvider, UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the currently rendered Umbraco page and passes provided querystring
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage(QueryString queryString)
        {
            return new RedirectToUmbracoPageResult(CurrentPage, queryString, _publishedUrlProvider, UmbracoContextAccessor);
        }

        /// <summary>
        /// Redirects to the currently rendered Umbraco URL
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is useful if you need to redirect
        /// to the current page but the current page is actually a rewritten URL normally done with something like
        /// Server.Transfer.*
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
        /// we need to recursively find the route definition based on the parent view context
        /// </summary>
        /// <returns></returns>
        private Attempt<RouteDefinition> TryGetRouteDefinitionFromAncestorViewContexts()
        {
            var currentContext = ControllerContext;
            while (!(currentContext is null))
            {
                var currentRouteData = currentContext.RouteData;
                if (currentRouteData.DataTokens.ContainsKey(Core.Constants.Web.UmbracoRouteDefinitionDataToken))
                    return Attempt.Succeed((RouteDefinition)currentRouteData.DataTokens[Core.Constants.Web.UmbracoRouteDefinitionDataToken]);
            }

            return Attempt<RouteDefinition>.Fail(
                new InvalidOperationException("Cannot find the Umbraco route definition in the route values, the request must be made in the context of an Umbraco request"));
        }
    }
}
