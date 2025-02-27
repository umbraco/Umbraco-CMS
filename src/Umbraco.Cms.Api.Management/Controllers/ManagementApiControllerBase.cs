using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Common.Mvc.ActionResults;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[Authorize(Policy = AuthorizationPolicies.UmbracoFeatureEnabled)]
[MapToApi(ManagementApiConfiguration.ApiName)]
[JsonOptionsName(Constants.JsonOptionsNames.BackOffice)]
[AppendEventMessages]
[Produces("application/json")]
public abstract class ManagementApiControllerBase : Controller, IUmbracoFeature
{
    protected IActionResult CreatedAtId<T>(Expression<Func<T, string>> action, Guid id)
        => CreatedAtAction(action, new { id = id }, id.ToString());

    protected IActionResult CreatedAtPath<T>(Expression<Func<T, string>> action, string path)
        => CreatedAtAction(action, new { path = path }, path);

    protected IActionResult CreatedAtAction<T>(Expression<Func<T, string>> action, object routeValues, string resourceIdentifier)
    {
        if (action.Body is not ConstantExpression constantExpression)
        {
            throw new ArgumentException("Expression must be a constant expression.");
        }

        var controllerName = ManagementApiRegexes.ControllerTypeToNameRegex().Replace(typeof(T).Name, string.Empty);
        var actionName = constantExpression.Value?.ToString() ?? throw new ArgumentException("Expression does not have a value.");

        return new EmptyCreatedAtActionResult(actionName, controllerName, routeValues, resourceIdentifier);
    }

    protected static Guid CurrentUserKey(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        => CurrentUser(backOfficeSecurityAccessor).Key;

    protected static IUser CurrentUser(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        => backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser ?? throw new InvalidOperationException("No backoffice user found");

    /// <summary>
    ///     Creates a 403 Forbidden result.
    /// </summary>
    /// <remarks>
    ///     Use this method instead of <see cref="ManagementApiControllerBase.Forbid()"/> on the controller base.
    ///     This method ensures that a proper 403 Forbidden status code is returned to the client.
    /// </remarks>
    // Duplicate code copied between Management API and Delivery API.
    protected IActionResult Forbidden() => new StatusCodeResult(StatusCodes.Status403Forbidden);

    protected static IActionResult OperationStatusResult<TEnum>(TEnum status, Func<ProblemDetailsBuilder, IActionResult> result)
        where TEnum : Enum
        => result(new ProblemDetailsBuilder().WithOperationStatus(status));

    protected BadRequestObjectResult SkipTakeToPagingProblem() =>
        BadRequest(new ProblemDetails
        {
            Title = "Invalid skip/take",
            Detail = "Skip must be a multiple of take - i.e. skip = 10, take = 5",
            Status = StatusCodes.Status400BadRequest,
            Type = "Error",
        });
}
