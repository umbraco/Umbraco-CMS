using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Common.Mvc.ActionResults;

/// <summary>
/// A "created at" action result with no response body.
/// </summary>
public sealed class EmptyCreatedAtActionResult : ActionResult
{
    private readonly string _actionName;
    private readonly string _controllerName;
    private readonly object _routeValues;
    private readonly string _resourceIdentifier;

    public EmptyCreatedAtActionResult(string actionName, string controllerName, object routeValues, string resourceIdentifier)
    {
        _actionName = actionName;
        _controllerName = controllerName;
        _routeValues = routeValues;
        _resourceIdentifier = resourceIdentifier;
    }

    public override void ExecuteResult(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        HttpRequest request = context.HttpContext.Request;
        IUrlHelper urlHelper = context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(context);

        var url = urlHelper.Action(
            _actionName,
            _controllerName,
            _routeValues,
            request.Scheme,
            request.Host.ToUriComponent());

        if (string.IsNullOrEmpty(url))
        {
            throw new InvalidOperationException("No routes could be found that matched the provided route components");
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        context.HttpContext.Response.Headers.Location = url;
        context.HttpContext.Response.Headers[Constants.Headers.GeneratedResource] = _resourceIdentifier;
    }
}
