using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
	//[PluginController("MyTestSurfaceController")]
	//public class TestSurfaceController : SurfaceController
	//{
	//    public ActionResult Index()
	//    {
	//        return View();
	//        //return Content("<html><body>hello</body></html>");
	//    }

	//    public ActionResult PostVals(string name)
	//    {
	//        ModelState.AddModelError("name", "bad name!");
	//        return CurrentUmbracoPage();
	//    }
	//}

	//public class LocalSurfaceController : SurfaceController
	//{
	//    public ActionResult Index()
	//    {
	//        return View();
	//    }

	//    public ActionResult PostVals([Bind(Prefix = "blah")]string name)
	//    {
	//        ModelState.AddModelError("name", "you suck!");
	//        return this.RedirectToCurrentUmbracoPage();
	//    }
	//}

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

		
	}
}