using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using static Umbraco.Cms.Core.Constants.Web.Routing;

namespace Umbraco.Cms.Web.Website.ActionResults;

/// <summary>
///     Used by posted forms to proxy the result to the page in which the current URL matches on
/// </summary>
/// <remarks>
///     This page does not redirect therefore it does not implement <see cref="IKeepTempDataResult" /> because TempData
///     should
///     only be used in situations when a redirect occurs. It is not good practice to use TempData when redirects do not
///     occur
///     so we'll be strict about it and not save it.
/// </remarks>
public class UmbracoPageResult : IActionResult
{
    private readonly IProfilingLogger _profilingLogger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoPageResult" /> class.
    /// </summary>
    public UmbracoPageResult(IProfilingLogger profilingLogger) => _profilingLogger = profilingLogger;

    /// <inheritdoc />
    public async Task ExecuteResultAsync(ActionContext context)
    {
        UmbracoRouteValues? umbracoRouteValues = context.HttpContext.Features.Get<UmbracoRouteValues>();
        if (umbracoRouteValues == null)
        {
            throw new InvalidOperationException(
                $"Can only use {nameof(UmbracoPageResult)} in the context of an Http POST when using a {nameof(SurfaceController)} form");
        }

        // Change the route values back to the original request vals
        context.RouteData.Values[ControllerToken] = umbracoRouteValues.ControllerName;
        context.RouteData.Values[ActionToken] = umbracoRouteValues.ActionName;

        // Create a new context and excute the original controller...
        // Copy the action context - this also copies the ModelState
        var renderActionContext = new ActionContext(context)
        {
            ActionDescriptor = umbracoRouteValues.ControllerActionDescriptor,
        };
        IActionInvokerFactory actionInvokerFactory =
            context.HttpContext.RequestServices.GetRequiredService<IActionInvokerFactory>();
        IActionInvoker? actionInvoker = actionInvokerFactory.CreateInvoker(renderActionContext);
        await ExecuteControllerAction(actionInvoker);
    }

    /// <summary>
    ///     Executes the controller action
    /// </summary>
    private async Task ExecuteControllerAction(IActionInvoker? actionInvoker)
    {
        using (_profilingLogger.TraceDuration<UmbracoPageResult>(
                   "Executing Umbraco RouteDefinition controller",
                   "Finished"))
        {
            if (actionInvoker is not null)
            {
                await actionInvoker.InvokeAsync();
            }
        }
    }
}
