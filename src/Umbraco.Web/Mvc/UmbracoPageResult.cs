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

            var targetController = ((Controller)routeDef.Controller);
		    var sourceController = ((Controller) context.Controller);

			//ensure the original template is reset
			context.RouteData.Values["action"] = routeDef.ActionName;

			//ensure ModelState is copied across
            routeDef.Controller.ViewData.ModelState.Merge(sourceController.ViewData.ModelState);

			//ensure TempData and ViewData is copied across
            foreach (var d in sourceController.ViewData)
				routeDef.Controller.ViewData[d.Key] = d.Value;


            //We cannot simply merge the temp data because during controller execution it will attempt to 'load' temp data
            // but since it has not been saved, there will be nothing to load and it will revert to nothing, so the trick is 
            // to Save the state of the temp data first then it will automatically be picked up.
            // http://issues.umbraco.org/issue/U4-1339
            targetController.TempDataProvider = sourceController.TempDataProvider;
		    targetController.TempData = sourceController.TempData;
            targetController.TempData.Save(sourceController.ControllerContext, sourceController.TempDataProvider);

			using (DisposableTimer.TraceDuration<UmbracoPageResult>("Executing Umbraco RouteDefinition controller", "Finished"))
			{
				try
				{
                    ((IController)targetController).Execute(context.RequestContext);
				}
				finally
				{
					routeDef.Controller.DisposeIfDisposable();
				}
			}

		}
	}
}