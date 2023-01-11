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
    {
        if (action.Body is not ConstantExpression constantExpression)
        {
            throw new ArgumentException("Expression must be a constant expression.");
        }

        var controllerName = ManagementApiRegexes.ControllerTypeToNameRegex().Replace(typeof(T).Name, string.Empty);
        var actionName = constantExpression.Value?.ToString() ?? throw new ArgumentException("Expression does not have a value.");

        return base.CreatedAtAction(actionName, controllerName, new { key = key }, null);
    }

    protected static int CurrentUserId(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        => backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? Core.Constants.Security.SuperUserId;
}
