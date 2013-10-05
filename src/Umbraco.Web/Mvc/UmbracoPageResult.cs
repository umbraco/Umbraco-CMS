using System;
using System.Web.Mvc;
using System.Web.Routing;
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
			ResetRouteData(context.RouteData);

            ValidateRouteData(context.RouteData);

			var routeDef = (RouteDefinition)context.RouteData.DataTokens["umbraco-route-def"];
            var factory = ControllerBuilder.Current.GetControllerFactory();

			context.RouteData.Values["action"] = routeDef.ActionName;

		    ControllerBase controller = null;

		    try
		    {
                controller = CreateController(context, factory, routeDef);
                
                controller.ViewData.ModelState.Merge(context.Controller.ViewData.ModelState);

                CopyControllerData(context, controller);

		        ExecuteControllerAction(context, controller);
		    }
		    finally
		    {
		        CleanupController(controller, factory);
		    }
		}

        /// <summary>
        /// Executes the controller action
        /// </summary>
	    private static void ExecuteControllerAction(ControllerContext context, IController controller)
	    {
	        using (DisposableTimer.TraceDuration<UmbracoPageResult>("Executing Umbraco RouteDefinition controller", "Finished"))
	        {
	            controller.Execute(context.RequestContext);
	        }
	    }
        
	    /// <summary>
        /// Since we could be returning the current page from a surface controller posted values in which the routing values are changed, we 
        /// need to revert these values back to nothing in order for the normal page to render again.
        /// </summary>
        private static void ResetRouteData(RouteData routeData)
	    {
            routeData.DataTokens["area"] = null;
            routeData.DataTokens["Namespaces"] = null;
	    }

        /// <summary>
        /// Validate that the current page execution is not being handled by the normal umbraco routing system
        /// </summary>
        private static void ValidateRouteData(RouteData routeData)
        {
            if (!routeData.DataTokens.ContainsKey("umbraco-route-def"))
            {
                throw new InvalidOperationException("Can only use " + typeof(UmbracoPageResult).Name +
                                                    " in the context of an Http POST when using a SurfaceController form");
            }
        }

        /// <summary>
        /// Ensure TempData and ViewData is copied across
        /// </summary>
        private static void CopyControllerData(ControllerContext context, ControllerBase controller)
        {
            foreach (var d in context.Controller.ViewData)
                controller.ViewData[d.Key] = d.Value;

            controller.TempData = context.Controller.TempData;
        }

        /// <summary>
        /// Creates a controller using the controller factory
        /// </summary>
        private static ControllerBase CreateController(ControllerContext context, IControllerFactory factory, RouteDefinition routeDef)
        {
            var controller = factory.CreateController(context.RequestContext, routeDef.ControllerName) as ControllerBase;

            if (controller == null)
                throw new InvalidOperationException("Could not create controller with name " + routeDef.ControllerName + ".");

            return controller;
        }

        /// <summary>
        /// Cleans up the controller by releasing it using the controller factory, and by disposing it.
        /// </summary>
        private static void CleanupController(IController controller, IControllerFactory factory)
        {
            if (controller != null)
                factory.ReleaseController(controller);

            if (controller != null)
                controller.DisposeIfDisposable();
        }
	}
}