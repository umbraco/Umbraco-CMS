using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
	[SurfaceController("DD307F95-6D90-4593-8C97-093AC7C12573")]
	public class TestSurfaceController : SurfaceController
	{
		public ActionResult Index()
		{
			return Content("<html><body>hello</body></html>");
		}
	}

	/// <summary>
	/// The base controller that all Presentation Add-in controllers should inherit from
	/// </summary>
	[MergeModelStateToChildAction]
	public abstract class SurfaceController : Controller, IRequiresUmbracoContext
	{
		///// <summary>
		///// stores the metadata about surface controllers
		///// </summary>
		//private static ConcurrentDictionary<Type, SurfaceControllerMetadata> _metadata = new ConcurrentDictionary<Type, SurfaceControllerMetadata>();

		public UmbracoContext UmbracoContext { get; set; }

		/// <summary>
		/// Useful for debugging
		/// </summary>
		internal Guid InstanceId { get; private set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="umbracoContext"></param>
		protected SurfaceController(UmbracoContext umbracoContext)
		{
			UmbracoContext = umbracoContext;
			InstanceId = Guid.NewGuid();
		}

		/// <summary>
		/// Empty constructor, uses Singleton to resolve the UmbracoContext
		/// </summary>
		protected SurfaceController()
		{
			UmbracoContext = UmbracoContext.Current;
			InstanceId = Guid.NewGuid();
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
		/// <param name="pageDocument"></param>
		/// <returns></returns>
		protected RedirectToUmbracoPageResult RedirectToUmbracoPage(IDocument pageDocument)
		{
			return new RedirectToUmbracoPageResult(pageDocument, UmbracoContext);
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
		protected IDocument CurrentPage
		{
			get
			{
				if (!ControllerContext.RouteData.DataTokens.ContainsKey("umbraco-route-def"))
					throw new InvalidOperationException("Can only use " + typeof(UmbracoPageResult).Name + " in the context of an Http POST when using the BeginUmbracoForm helper");

				var routeDef = (RouteDefinition)ControllerContext.RouteData.DataTokens["umbraco-route-def"];
				return routeDef.DocumentRequest.Document;
			}
		}

		/// <summary>
		/// Returns the metadata for this instance
		/// </summary>
		internal SurfaceControllerMetadata GetMetadata()
		{
			var controllerId = this.GetType().GetCustomAttribute<SurfaceControllerAttribute>(false);

			return new SurfaceControllerMetadata()
				{
					ControllerId = controllerId == null ? null : (Guid?) controllerId.Id,
					ControllerName = ControllerExtensions.GetControllerName(this.GetType()),
					ControllerNamespace = this.GetType().Namespace,
					ControllerType = this.GetType()
				};
		}
	}
}