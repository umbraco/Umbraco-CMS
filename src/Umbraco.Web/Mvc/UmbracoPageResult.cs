using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Used by posted forms to proxy the result to the page in which the current URL matches on
	/// </summary>
	public class UmbracoPageResult : ActionResult
	{
	    private readonly ProfilingLogger _profilingLogger;

	    public UmbracoPageResult(ProfilingLogger profilingLogger)
	    {
	        _profilingLogger = profilingLogger;
	    }

        [Obsolete("Use the ctor specifying all depenendencies instead")]
	    public UmbracoPageResult()
            : this(new ProfilingLogger(LoggerResolver.Current.Logger, ProfilerResolver.Current.Profiler))
	    {
	        
	    }

	    public override void ExecuteResult(ControllerContext context)
		{
			ResetRouteData(context.RouteData);

            ValidateRouteData(context.RouteData);

			var routeDef = (RouteDefinition)context.RouteData.DataTokens["umbraco-route-def"];

            //Special case, if it is webforms but we're posting to an MVC surface controller, then we 
            // need to return the webforms result instead
		    if (routeDef.PublishedContentRequest.RenderingEngine == RenderingEngine.WebForms)
		    {
		        EnsureViewContextForWebForms(context);
		        var webFormsHandler = RenderRouteHandler.GetWebFormsHandler();
		        webFormsHandler.ProcessRequest(HttpContext.Current);
		    }
		    else
		    {
                var factory = ControllerBuilder.Current.GetControllerFactory();
                context.RouteData.Values["action"] = routeDef.ActionName;
                ControllerBase controller = null;

                try
                {
                    controller = CreateController(context, factory, routeDef);

                    CopyControllerData(context, controller);

                    ExecuteControllerAction(context, controller);
                }
                finally
                {
                    CleanupController(controller, factory);
                }    
		    }
		}

        /// <summary>
        /// Executes the controller action
        /// </summary>
	    private void ExecuteControllerAction(ControllerContext context, IController controller)
	    {
            using (_profilingLogger.TraceDuration<UmbracoPageResult>("Executing Umbraco RouteDefinition controller", "Finished"))
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
            if (routeData.DataTokens.ContainsKey("umbraco-route-def") == false)
            {
                throw new InvalidOperationException("Can only use " + typeof(UmbracoPageResult).Name +
                                                    " in the context of an Http POST when using a SurfaceController form");
            }
        }

        /// <summary>
        /// When POSTing to MVC but rendering in WebForms we need to do some trickery, we'll create a dummy viewcontext with all of the
        /// current modelstate, tempdata, viewdata so that if we're rendering partial view macros within the webforms view, they will
        /// get all of this merged into them.
        /// </summary>
        /// <param name="context"></param>
        private static void EnsureViewContextForWebForms(ControllerContext context)
        {
            var tempDataDictionary = new TempDataDictionary();
            tempDataDictionary.Save(context, new SessionStateTempDataProvider());
            var viewCtx = new ViewContext(context, new DummyView(), new ViewDataDictionary(), tempDataDictionary, new StringWriter());

            viewCtx.ViewData.ModelState.Merge(context.Controller.ViewData.ModelState);

            foreach (var d in context.Controller.ViewData)
                viewCtx.ViewData[d.Key] = d.Value;

            //now we need to add it to the special route tokens so it's picked up later
            context.HttpContext.Request.RequestContext.RouteData.DataTokens[Constants.DataTokenCurrentViewContext] = viewCtx;
        }

        /// <summary>
        /// Ensure ModelState, ViewData and TempData is copied across
        /// </summary>
        private static void CopyControllerData(ControllerContext context, ControllerBase controller)
        {
            controller.ViewData.ModelState.Merge(context.Controller.ViewData.ModelState);

            foreach (var d in context.Controller.ViewData)
                controller.ViewData[d.Key] = d.Value;

            //We cannot simply merge the temp data because during controller execution it will attempt to 'load' temp data
            // but since it has not been saved, there will be nothing to load and it will revert to nothing, so the trick is 
            // to Save the state of the temp data first then it will automatically be picked up.
            // http://issues.umbraco.org/issue/U4-1339

            var targetController = controller as Controller;
            var sourceController = context.Controller as Controller;
            if (targetController != null && sourceController != null)
            {
                targetController.TempDataProvider = sourceController.TempDataProvider;
                targetController.TempData = sourceController.TempData;
                targetController.TempData.Save(sourceController.ControllerContext, sourceController.TempDataProvider);    
            }
            
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

	    private class DummyView : IView
	    {
	        public void Render(ViewContext viewContext, TextWriter writer)
	        {
	        }
	    }
	}
}