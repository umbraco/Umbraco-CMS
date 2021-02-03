using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Extensions;
using Umbraco.Web.Common.Routing;

namespace Umbraco.Web.Website.ActionResults
{
    /// <summary>
    /// Used by posted forms to proxy the result to the page in which the current URL matches on
    /// </summary>
    public class UmbracoPageResult : IActionResult
    {
        private readonly IProfilingLogger _profilingLogger;

        public UmbracoPageResult(IProfilingLogger profilingLogger)
        {
            _profilingLogger = profilingLogger;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var routeData = context.RouteData;

            ResetRouteData(routeData);
            ValidateRouteData(context);

            IControllerFactory factory = context.HttpContext.RequestServices.GetRequiredService<IControllerFactory>();
            Controller controller = null;

            if (!(context is ControllerContext controllerContext))
            {
                return Task.FromCanceled(CancellationToken.None);
            }

            try
            {
                controller = CreateController(controllerContext, factory);

                CopyControllerData(controllerContext, controller);

                ExecuteControllerAction(controllerContext, controller);
            }
            finally
            {
                CleanupController(controllerContext, controller, factory);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes the controller action
        /// </summary>
        private void ExecuteControllerAction(ControllerContext context, Controller controller)
        {
            using (_profilingLogger.TraceDuration<UmbracoPageResult>("Executing Umbraco RouteDefinition controller", "Finished"))
            {
                //TODO I do not think this will work, We need to test this, when we can, in the .NET Core executable.
                var aec = new ActionExecutingContext(context, new List<IFilterMetadata>(), new Dictionary<string, object>(), controller);
                var actionExecutedDelegate = CreateActionExecutedDelegate(aec);

                controller.OnActionExecutionAsync(aec, actionExecutedDelegate);
            }
        }

        /// <summary>
        /// Creates action execution delegate from ActionExecutingContext
        /// </summary>
        private static ActionExecutionDelegate CreateActionExecutedDelegate(ActionExecutingContext context)
        {
            var actionExecutedContext = new ActionExecutedContext(context, context.Filters, context.Controller)
            {
                Result = context.Result,
            };
            return () => Task.FromResult(actionExecutedContext);
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
        private static void ValidateRouteData(ActionContext actionContext)
        {
            UmbracoRouteValues umbracoRouteValues = actionContext.HttpContext.Features.Get<UmbracoRouteValues>();
            if (umbracoRouteValues == null)
            {
                throw new InvalidOperationException("Can only use " + typeof(UmbracoPageResult).Name +
                                                    " in the context of an Http POST when using a SurfaceController form");
            }
        }

        /// <summary>
        /// Ensure ModelState, ViewData and TempData is copied across
        /// </summary>
        private static void CopyControllerData(ControllerContext context, Controller controller)
        {
            controller.ViewData.ModelState.Merge(context.ModelState);

            foreach (var d in controller.ViewData)
            {
                controller.ViewData[d.Key] = d.Value;
            }

            // We cannot simply merge the temp data because during controller execution it will attempt to 'load' temp data
            // but since it has not been saved, there will be nothing to load and it will revert to nothing, so the trick is
            // to Save the state of the temp data first then it will automatically be picked up.
            // http://issues.umbraco.org/issue/U4-1339

            var targetController = controller;
            var tempDataDictionaryFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
            var tempData = tempDataDictionaryFactory.GetTempData(context.HttpContext);

            targetController.TempData = tempData;
            targetController.TempData.Save();
        }

        /// <summary>
        /// Creates a controller using the controller factory
        /// </summary>
        private static Controller CreateController(ControllerContext context, IControllerFactory factory)
        {
            if (!(factory.CreateController(context) is Controller controller))
            {
                throw new InvalidOperationException("Could not create controller with name " + context.ActionDescriptor.ControllerName + ".");
            }

            return controller;
        }

        /// <summary>
        /// Cleans up the controller by releasing it using the controller factory, and by disposing it.
        /// </summary>
        private static void CleanupController(ControllerContext context, Controller controller, IControllerFactory factory)
        {
            if (!(controller is null))
            {
                factory.ReleaseController(context, controller);
            }

            controller?.DisposeIfDisposable();
        }

        private class DummyView : IView
        {
            public DummyView(string path)
            {
                Path = path;
            }

            public Task RenderAsync(ViewContext context)
            {
                return Task.CompletedTask;
            }

            public string Path { get; }
        }
    }
}
