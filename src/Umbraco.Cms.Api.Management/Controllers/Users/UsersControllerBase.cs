using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

[ApiController]
[VersionedApiBackOfficeRoute("users")]
[ApiExplorerSettings(GroupName = "Users")]
[ApiVersion("1.0")]
public class UsersControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserOperationStatusResult(UserOperationStatus status) =>
        status switch
        {
            UserOperationStatus.Success => Ok(),
            UserOperationStatus.MissingUser => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Missing user")
                .WithDetail("A performing user is required for the operation, but none was found.")
                .Build()),
            UserOperationStatus.MissingUserGroup => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Missing user group")
                .WithDetail("The specified user group was not found.")
                .Build()),
            _ =>StatusCode(StatusCodes.Status500InternalServerError, "Unknown user group operation status."),
        };
}
