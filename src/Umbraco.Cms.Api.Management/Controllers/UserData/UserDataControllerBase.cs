using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserData;

[VersionedApiBackOfficeRoute("user-data")]
[ApiExplorerSettings(GroupName = "User Data")]
public class UserDataControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserDataOperationStatusResult(UserDataOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            UserDataOperationStatus.Success => Ok(),
            UserDataOperationStatus.UserNotFound => NotFound(problemDetailsBuilder
                .WithTitle("User not found")
                .Build()),
            UserDataOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("UserData not found")
                .Build()),
            UserDataOperationStatus.AlreadyExists => BadRequest(problemDetailsBuilder
                .WithTitle("UserData already exists")
                .WithDetail("A userData entry with the given key already exists")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown userData operation status.")
                .Build()),
        });
}
