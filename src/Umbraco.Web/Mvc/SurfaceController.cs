using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;
using Umbraco.Web.Security;

namespace Umbraco.Web.Mvc
{

    /// <summary>
    /// The base controller that all Presentation Add-in controllers should inherit from
    /// </summary>
    [MergeModelStateToChildAction]
    [MergeParentContextViewData]
    public abstract class SurfaceController : PluginController
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        protected SurfaceController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
            _membershipHelper = new MembershipHelper(umbracoContext);
        }

        /// <summary>
        /// Empty constructor, uses Singleton to resolve the UmbracoContext
        /// </summary>
        protected SurfaceController()
            : base(UmbracoContext.Current)
        {
            _membershipHelper = new MembershipHelper(UmbracoContext.Current);
        }

        private readonly MembershipHelper _membershipHelper;

        /// <summary>
        /// Returns the MemberHelper instance
        /// </summary>
        public MembershipHelper Members
        {
            get { return _membershipHelper; }
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(int pageId)
        {
            return new RedirectToUmbracoPageResult(pageId, UmbracoContext);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IPublishedContent publishedContent)
        {
            return new RedirectToUmbracoPageResult(publishedContent, UmbracoContext);
        }

        /// <summary>
        /// Redirects to the currently rendered Umbraco page
        /// </summary>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage()
        {
            return new RedirectToUmbracoPageResult(CurrentPage, UmbracoContext);
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
            return new UmbracoPageResult(ApplicationContext.ProfilingLogger);
        }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        protected IPublishedContent CurrentPage
        {
            get
            {
			    var routeDefAttempt = TryGetRouteDefinitionFromAncestorViewContexts();
                if (!routeDefAttempt.Success)
                {
                    throw routeDefAttempt.Exception;
                }

			    var routeDef = routeDefAttempt.Result;
                return routeDef.PublishedContentRequest.PublishedContent;
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
            ControllerContext currentContext = ControllerContext;
            while (currentContext != null)
            {
                var currentRouteData = currentContext.RouteData;
                if (currentRouteData.DataTokens.ContainsKey("umbraco-route-def"))
                {
                    return Attempt.Succeed((RouteDefinition) currentRouteData.DataTokens["umbraco-route-def"]);
                }
                if (currentContext.IsChildAction)
                {
                    //assign current context to parent
                    currentContext = currentContext.ParentActionViewContext;
                }
                else
                {
                    //exit the loop
                    currentContext = null;
                }
            }
            return Attempt<RouteDefinition>.Fail(
                new InvalidOperationException("Cannot find the Umbraco route definition in the route values, the request must be made in the context of an Umbraco request"));
        } 
        

    }
}