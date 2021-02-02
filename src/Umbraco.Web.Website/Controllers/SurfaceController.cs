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
using Umbraco.Web.Common.Routing;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="SurfaceController"/> class.
        /// </summary>
        protected SurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger)
            => PublishedUrlProvider = publishedUrlProvider;

        protected IPublishedUrlProvider PublishedUrlProvider { get; }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        protected virtual IPublishedContent CurrentPage
        {
            get
            {
                var routeDefAttempt = TryGetRouteDefinitionFromAncestorViewContexts();
                if (routeDefAttempt.Success == false)
                {
                    throw routeDefAttempt.Exception;
                }

                var routeDef = routeDefAttempt.Result;
                return routeDef.PublishedRequest.PublishedContent;
            }
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id
        /// </summary>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(Guid contentKey)
            => new RedirectToUmbracoPageResult(contentKey, PublishedUrlProvider, UmbracoContextAccessor);

        /// <summary>
        /// Redirects to the Umbraco page with the given id and passes provided querystring
        /// </summary>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(Guid contentKey, QueryString queryString)
            => new RedirectToUmbracoPageResult(contentKey, queryString, PublishedUrlProvider, UmbracoContextAccessor);

        /// <summary>
        /// Redirects to the Umbraco page with the given published content
        /// </summary>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IPublishedContent publishedContent)
            => new RedirectToUmbracoPageResult(publishedContent, PublishedUrlProvider, UmbracoContextAccessor);

        /// <summary>
        /// Redirects to the Umbraco page with the given published content and passes provided querystring
        /// </summary>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IPublishedContent publishedContent, QueryString queryString)
            => new RedirectToUmbracoPageResult(publishedContent, queryString, PublishedUrlProvider, UmbracoContextAccessor);

        /// <summary>
        /// Redirects to the currently rendered Umbraco page
        /// </summary>
        protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage()
            => new RedirectToUmbracoPageResult(CurrentPage, PublishedUrlProvider, UmbracoContextAccessor);

        /// <summary>
        /// Redirects to the currently rendered Umbraco page and passes provided querystring
        /// </summary>
        protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage(QueryString queryString)
            => new RedirectToUmbracoPageResult(CurrentPage, queryString, PublishedUrlProvider, UmbracoContextAccessor);

        /// <summary>
        /// Redirects to the currently rendered Umbraco URL
        /// </summary>
        /// <remarks>
        /// This is useful if you need to redirect
        /// to the current page but the current page is actually a rewritten URL normally done with something like
        /// Server.Transfer.*
        /// </remarks>
        protected RedirectToUmbracoUrlResult RedirectToCurrentUmbracoUrl()
            => new RedirectToUmbracoUrlResult(UmbracoContext);

        /// <summary>
        /// Returns the currently rendered Umbraco page
        /// </summary>
        protected UmbracoPageResult CurrentUmbracoPage()
            => new UmbracoPageResult(ProfilingLogger);

        /// <summary>
        /// we need to recursively find the route definition based on the parent view context
        /// </summary>
        private Attempt<UmbracoRouteValues> TryGetRouteDefinitionFromAncestorViewContexts()
        {
            var currentContext = ControllerContext;
            while (!(currentContext is null))
            {
                var currentRouteData = currentContext.RouteData;
                if (currentRouteData.Values.ContainsKey(Constants.Web.UmbracoRouteDefinitionDataToken))
                {
                    return Attempt.Succeed((UmbracoRouteValues)currentRouteData.Values[Constants.Web.UmbracoRouteDefinitionDataToken]);
                }
            }

            return Attempt<UmbracoRouteValues>.Fail(
                new InvalidOperationException("Cannot find the Umbraco route definition in the route values, the request must be made in the context of an Umbraco request"));
        }
    }
}
