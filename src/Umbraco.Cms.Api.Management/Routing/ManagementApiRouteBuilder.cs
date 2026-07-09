using System.Linq.Expressions;
using Asp.Versioning;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Controllers;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Provides URL generation for Management API controllers using LinkGenerator.
/// </summary>
/// <remarks>
/// This service mirrors the CreatedAtId pattern used in ManagementApiControllerBase
/// but works in non-controller contexts where IUrlHelper is not available.
/// </remarks>
internal sealed class ManagementApiRouteBuilder : IManagementApiRouteBuilder
{
    private readonly LinkGenerator _linkGenerator;
    private readonly ApiVersioningOptions _apiVersioningOptions;

    public ManagementApiRouteBuilder(
        LinkGenerator linkGenerator,
        IOptions<ApiVersioningOptions> apiVersioningOptions)
    {
        _linkGenerator = linkGenerator;
        _apiVersioningOptions = apiVersioningOptions.Value;
    }

    /// <inheritdoc />
    public string? GetPathByAction<TController>(
        Expression<Func<TController, string>> action,
        object? routeValues = null)
    {
        if (action.Body is not ConstantExpression constantExpression)
        {
            throw new ArgumentException("Expression must be a constant expression.", nameof(action));
        }

        var controllerName = ManagementApiRegexes.ControllerTypeToNameRegex()
            .Replace(typeof(TController).Name, string.Empty);
        var actionName = constantExpression.Value?.ToString()
            ?? throw new ArgumentException("Expression does not have a value.", nameof(action));

        // Merge provided route values with the required API version
        var allRouteValues = new RouteValueDictionary(routeValues)
        {
            ["version"] = _apiVersioningOptions.DefaultApiVersion.MajorVersion?.ToString(),
        };

        return _linkGenerator.GetPathByAction(actionName, controllerName, allRouteValues);
    }
}
