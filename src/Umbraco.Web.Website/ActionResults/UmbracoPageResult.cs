using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Extensions;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Website.Controllers;
using Umbraco.Web.Website.Routing;

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

        /// <inheritdoc/>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            UmbracoRouteValues umbracoRouteValues = context.HttpContext.Features.Get<UmbracoRouteValues>();
            if (umbracoRouteValues == null)
            {
                throw new InvalidOperationException($"Can only use {nameof(UmbracoPageResult)} in the context of an Http POST when using a {nameof(SurfaceController)} form");
            }

            // Change the route values back to the original request vals
            context.RouteData.Values[UmbracoRouteValueTransformer.ControllerToken] = umbracoRouteValues.ControllerName;
            context.RouteData.Values[UmbracoRouteValueTransformer.ActionToken] = umbracoRouteValues.ActionName;

            // Create a new context and excute the original controller

            // TODO: We need to take into account temp data, view data, etc... all like what we used to do below
            // so that validation stuff gets carried accross

            var renderActionContext = new ActionContext(context.HttpContext, context.RouteData, umbracoRouteValues.ControllerActionDescriptor);
            IActionInvokerFactory actionInvokerFactory = context.HttpContext.RequestServices.GetRequiredService<IActionInvokerFactory>();
            IActionInvoker actionInvoker = actionInvokerFactory.CreateInvoker(renderActionContext);
            await ExecuteControllerAction(actionInvoker);

            //ResetRouteData(context.RouteData);
            //ValidateRouteData(context);

            //IControllerFactory factory = context.HttpContext.RequestServices.GetRequiredService<IControllerFactory>();
            //Controller controller = null;

            //if (!(context is ControllerContext controllerContext))
            //{
            //    // TODO: Better to throw since this is not expected?
            //    return Task.FromCanceled(CancellationToken.None);
            //}

            //try
            //{
            //    controller = CreateController(controllerContext, factory);

            //    CopyControllerData(controllerContext, controller);

            //    ExecuteControllerAction(controllerContext, controller);
            //}
            //finally
            //{
            //    CleanupController(controllerContext, controller, factory);
            //}

            //return Task.CompletedTask;
        }

        /// <summary>
        /// Executes the controller action
        /// </summary>
        private async Task ExecuteControllerAction(IActionInvoker actionInvoker)
        {
            using (_profilingLogger.TraceDuration<UmbracoPageResult>("Executing Umbraco RouteDefinition controller", "Finished"))
            {
                await actionInvoker.InvokeAsync();
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
        /// Ensure ModelState, ViewData and TempData is copied across
        /// </summary>
        private static void CopyControllerData(ControllerContext context, Controller controller)
        {
            controller.ViewData.ModelState.Merge(context.ModelState);

            foreach (KeyValuePair<string, object> d in controller.ViewData)
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
