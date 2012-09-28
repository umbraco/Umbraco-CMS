using System;
using System.Web.Mvc;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Used by posted forms to proxy the result to the page in which the current URL matches on
	/// </summary>
	public class UmbracoPageResult : ActionResult
	{
		public override void ExecuteResult(ControllerContext context)
		{

			//since we could be returning the current page from a surface controller posted values in which the routing values are changed, we 
			//need to revert these values back to nothing in order for the normal page to render again.
			context.RouteData.DataTokens["area"] = null;
			context.RouteData.DataTokens["Namespaces"] = null;

			//validate that the current page execution is not being handled by the normal umbraco routing system
			if (!context.RouteData.DataTokens.ContainsKey("umbraco-route-def"))
			{
				throw new InvalidOperationException("Can only use " + typeof(UmbracoPageResult).Name + " in the context of an Http POST when using a SurfaceController form");
			}

			var routeDef = (RouteDefinition)context.RouteData.DataTokens["umbraco-route-def"];

			//ensure the original template is reset
			context.RouteData.Values["action"] = routeDef.ActionName;

			//ensure ModelState is copied across
			routeDef.Controller.ViewData.ModelState.Merge(context.Controller.ViewData.ModelState);

			//ensure TempData and ViewData is copied across
			foreach (var d in context.Controller.ViewData)
				routeDef.Controller.ViewData[d.Key] = d.Value;
			routeDef.Controller.TempData = context.Controller.TempData;

			using (DisposableTimer.TraceDuration<UmbracoPageResult>("Executing Umbraco RouteDefinition controller", "Finished"))
			{
				try
				{
					((IController)routeDef.Controller).Execute(context.RequestContext);
				}
				finally
				{
					routeDef.Controller.DisposeIfDisposable();
				}
			}

		}
	}
}