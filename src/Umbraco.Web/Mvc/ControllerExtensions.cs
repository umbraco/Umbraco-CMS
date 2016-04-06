using System;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    internal static class ControllerExtensions
    {
        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
		internal static string GetControllerName(Type controllerType)
        {
            if (!controllerType.Name.EndsWith("Controller"))
            {
                throw new InvalidOperationException("The controller type " + controllerType + " does not follow conventions, MVC Controller class names must be suffixed with the term 'Controller'");
            }
            return controllerType.Name.Substring(0, controllerType.Name.LastIndexOf("Controller"));
        }

        /// <summary>
        /// Return the controller name from the controller instance
        /// </summary>
        /// <param name="controllerInstance"></param>
        /// <returns></returns>
	    internal static string GetControllerName(this IController controllerInstance)
	    {
	        return GetControllerName(controllerInstance.GetType());
	    }

	    /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
		internal static string GetControllerName<T>()
        {
            return GetControllerName(typeof(T));
        }

		/// <summary>
		/// This is generally used for proxying to a ChildAction which requires a ViewContext to be setup
		/// but since the View isn't actually rendered the IView object is null, however the rest of the 
		/// properties are filled in.
		/// </summary>
		/// <param name="controller"></param>
		/// <returns></returns>
		internal static ViewContext CreateEmptyViewContext(this ControllerBase controller)
		{
			return new ViewContext
			{
				Controller = controller,
				HttpContext = controller.ControllerContext.HttpContext,
				RequestContext = controller.ControllerContext.RequestContext,
				RouteData = controller.ControllerContext.RouteData,
				TempData = controller.TempData,
				ViewData = controller.ViewData
			};
		}

		/// <summary>
		/// Returns the string output from a ViewResultBase object
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="viewResult"></param>
		/// <returns></returns>
		internal static string RenderViewResultAsString(this ControllerBase controller, ViewResultBase viewResult)
		{
			using (var sw = new StringWriter())
			{
				controller.EnsureViewObjectDataOnResult(viewResult);

				var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, viewResult.ViewData, viewResult.TempData, sw);
				viewResult.View.Render(viewContext, sw);
				foreach (var v in viewResult.ViewEngineCollection)
				{
					v.ReleaseView(controller.ControllerContext, viewResult.View);
				}
				return sw.ToString().Trim();
			}
		}
		
		/// <summary>
		/// Renders the partial view to string.
		/// </summary>
		/// <param name="controller">The controller context.</param>
		/// <param name="viewName">Name of the view.</param>
		/// <param name="model">The model.</param>
		/// <param name="isPartial">true if it is a Partial view, otherwise false for a normal view </param>
		/// <returns></returns>
		internal static string RenderViewToString(this ControllerBase controller, string viewName, object model, bool isPartial = false)
		{
			if (controller.ControllerContext == null)
				throw new ArgumentException("The controller must have an assigned ControllerContext to execute this method.");

			controller.ViewData.Model = model;

			using (var sw = new StringWriter())
			{
				var viewResult = !isPartial 
					? ViewEngines.Engines.FindView(controller.ControllerContext, viewName, null) 
					: ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
				var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
				viewResult.View.Render(viewContext, sw);
				viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);				
				return sw.GetStringBuilder().ToString();
			}
		}

        /// <summary>
        /// Normally in MVC the way that the View object gets assigned to the result is to Execute the ViewResult, this however
        /// will write to the Response output stream which isn't what we want. Instead, this method will use the same logic inside
        /// of MVC to assign the View object to the result but without executing it.
        /// This is only relavent for view results of PartialViewResult or ViewResult.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="controller"></param>
        private static void EnsureViewObjectDataOnResult(this ControllerBase controller, ViewResultBase result)
        {            
            if (result.View != null) return;

            if (string.IsNullOrEmpty(result.ViewName))
                result.ViewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            if (result.View != null) return;

            if (result is PartialViewResult)
            {
                var viewEngineResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, result.ViewName);

                if (viewEngineResult.View == null)
                {
                    throw new InvalidOperationException("Could not find the view " + result.ViewName + ", the following locations were searched: " + Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
                }

                result.View = viewEngineResult.View;
            }
            else if (result is ViewResult)
            {
                var vr = (ViewResult)result;
                var viewEngineResult = ViewEngines.Engines.FindView(controller.ControllerContext, vr.ViewName, vr.MasterName);

                if (viewEngineResult.View == null)
                {
                    throw new InvalidOperationException("Could not find the view " + vr.ViewName + ", the following locations were searched: " + Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
                }

                result.View = viewEngineResult.View;
            }
        }
    }
}
