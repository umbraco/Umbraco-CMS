using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Web.Common.Controllers;
using static Umbraco.Cms.Core.Constants.Web.Routing;

namespace Umbraco.Cms.Web.Website.Routing;

/// <summary>
///     Used to find a controller/action in the current available routes
/// </summary>
public class ControllerActionSearcher : IControllerActionSearcher
{
    private const string DefaultActionName = nameof(RenderController.Index);
    private readonly IActionSelector _actionSelector;
    private readonly ILogger<ControllerActionSearcher> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ControllerActionSearcher" /> class.
    /// </summary>
    public ControllerActionSearcher(
        ILogger<ControllerActionSearcher> logger,
        IActionSelector actionSelector)
    {
        _logger = logger;
        _actionSelector = actionSelector;
    }

    /// <summary>
    ///     Determines if a custom controller can hijack the current route
    /// </summary>
    /// <typeparam name="T">The controller type to find</typeparam>
    public ControllerActionDescriptor? Find<T>(HttpContext httpContext, string? controller, string? action) =>
        Find<T>(httpContext, controller, action, null);

    /// <summary>
    ///     Determines if a custom controller can hijack the current route
    /// </summary>
    /// <typeparam name="T">The controller type to find</typeparam>
    public ControllerActionDescriptor? Find<T>(HttpContext httpContext, string? controller, string? action, string? area)
    {
        IReadOnlyList<ControllerActionDescriptor>? candidates =
            FindControllerCandidates<T>(httpContext, controller, action, DefaultActionName, area);

        if (candidates?.Count > 0)
        {
            return candidates[0];
        }

        return null;
    }

    /// <summary>
    ///     Return a list of controller candidates that match the custom controller and action names
    /// </summary>
    private IReadOnlyList<ControllerActionDescriptor>? FindControllerCandidates<T>(
        HttpContext httpContext,
        string? customControllerName,
        string? customActionName,
        string? defaultActionName,
        string? area = null)
    {
        // Use aspnetcore's IActionSelector to do the finding since it uses an optimized cache lookup
        var routeValues = new RouteValueDictionary
        {
            [ControllerToken] = customControllerName,
            [ActionToken] = customActionName, // first try to find the custom action
        };

        if (area != null)
        {
            routeValues[AreaToken] = area;
        }

        var routeData = new RouteData(routeValues);
        var routeContext = new RouteContext(httpContext) { RouteData = routeData };

        // try finding candidates for the custom action
        var candidates = _actionSelector.SelectCandidates(routeContext)?
            .Cast<ControllerActionDescriptor>()
            .Where(x => TypeHelper.IsTypeAssignableFrom<T>(x.ControllerTypeInfo))
            .ToList();

        if (candidates?.Count > 0)
        {
            // return them if found
            return candidates;
        }

        // now find for the default action since we couldn't find the custom one
        routeValues[ActionToken] = defaultActionName;
        candidates = _actionSelector.SelectCandidates(routeContext)?
            .Cast<ControllerActionDescriptor>()
            .Where(x => TypeHelper.IsTypeAssignableFrom<T>(x.ControllerTypeInfo))
            .ToList();

        return candidates;
    }
}
