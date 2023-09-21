using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers;

[Authorize(Policy = "New" + AuthorizationPolicies.BackOfficeAccess)]
[MapToApi(ManagementApiConfiguration.ApiName)]
[JsonOptionsName(Constants.JsonOptionsNames.BackOffice)]
public abstract class ManagementApiControllerBase : Controller
{
    protected CreatedAtActionResult CreatedAtAction<T>(Expression<Func<T, string>> action, Guid id)
        => CreatedAtAction(action, new { id = id });

    protected CreatedAtActionResult CreatedAtAction<T>(Expression<Func<T, string>> action, object routeValues)
    {
        if (action.Body is not ConstantExpression constantExpression)
        {
            throw new ArgumentException("Expression must be a constant expression.");
        }

        var controllerName = ManagementApiRegexes.ControllerTypeToNameRegex().Replace(typeof(T).Name, string.Empty);
        var actionName = constantExpression.Value?.ToString() ?? throw new ArgumentException("Expression does not have a value.");

        return base.CreatedAtAction(actionName, controllerName, routeValues, null);
    }

    protected CreatedAtActionResult CreatedAtAction<T>(Expression<Func<T, string>> action, string name)
    {
        if (action.Body is not ConstantExpression constantExpression)
        {
            throw new ArgumentException("Expression must be a constant expression.");
        }

        var controllerName = ManagementApiRegexes.ControllerTypeToNameRegex().Replace(typeof(T).Name, string.Empty);
        var actionName = constantExpression.Value?.ToString() ?? throw new ArgumentException("Expression does not have a value.");

        return base.CreatedAtAction(actionName, controllerName, new { name = name }, null);
    }

    protected static Guid CurrentUserKey(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        return backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key ?? throw new InvalidOperationException("No backoffice user found");
    }

    /// <summary>
    ///     Creates a 403 Forbidden result.
    /// </summary>
    /// <remarks>
    ///     Use this method instead of <see cref="ManagementApiControllerBase.Forbid()"/> on the controller base.
    ///     This method ensures that a proper 403 Forbidden status code is returned to the client.
    /// </remarks>
    // Duplicate code copied between Management API and Delivery API.
    protected IActionResult Forbidden() => new StatusCodeResult(StatusCodes.Status403Forbidden);
}
