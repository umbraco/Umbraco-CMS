using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{

	/// <summary>
	/// The base controller that all Presentation Add-in controllers should inherit from
	/// </summary>
	[MergeModelStateToChildAction]
	public abstract class SurfaceController : PluginController
	{		

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="umbracoContext"></param>
		protected SurfaceController(UmbracoContext umbracoContext)
			: base(umbracoContext)
		{			
		}

		/// <summary>
		/// Empty constructor, uses Singleton to resolve the UmbracoContext
		/// </summary>
		protected SurfaceController()
			: base(UmbracoContext.Current)
		{			
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
		/// Returns the currently rendered Umbraco page
		/// </summary>
		/// <returns></returns>
		protected UmbracoPageResult CurrentUmbracoPage()
		{
			return new UmbracoPageResult();
		}

		/// <summary>
		/// Gets the current page.
		/// </summary>
		protected IPublishedContent CurrentPage
		{
			get
			{
                var routeData = ControllerContext.IsChildAction 
                    ? ControllerContext.ParentActionViewContext.RouteData 
                    : ControllerContext.RouteData;

                if (!routeData.DataTokens.ContainsKey("umbraco-route-def"))
                {
                    throw new InvalidOperationException("Cannot find the Umbraco route definition in the route values, the request must be made in the context of an Umbraco request");                    
                }

                var routeDef = (RouteDefinition)routeData.DataTokens["umbraco-route-def"];
				return routeDef.PublishedContentRequest.PublishedContent;
			}
		}

		
	}
}