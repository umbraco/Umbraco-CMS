using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Core.Security;
using Umbraco.New.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers;

[JsonOptionsName(Constants.JsonOptionsNames.BackOffice)]
public class ManagementApiControllerBase : Controller
{
    protected CreatedAtActionResult CreatedAtAction<T>(Expression<Func<T, string>> action, Guid key)
        => CreatedAtAction(action, new { key = key });

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
        => backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key ?? Core.Constants.Security.SuperUserKey;
}
