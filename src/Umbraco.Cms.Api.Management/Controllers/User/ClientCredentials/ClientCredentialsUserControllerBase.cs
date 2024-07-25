using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.ClientCredentials;

[ApiExplorerSettings(GroupName = "User")]
public abstract class ClientCredentialsUserControllerBase : UserControllerBase
{
    protected IActionResult BackOfficeUserClientCredentialsOperationStatusResult(BackOfficeUserClientCredentialsOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            BackOfficeUserClientCredentialsOperationStatus.InvalidUser => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid user")
                .WithDetail("The specified user does not support this operation. Possibly caused by a mismatched client ID or an inapplicable user type.")
                .Build()),
            BackOfficeUserClientCredentialsOperationStatus.DuplicateClientId => BadRequest(problemDetailsBuilder
                .WithTitle("Duplicate client ID")
                .WithDetail("The specified client ID is already in use. Choose another client ID.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown client credentials operation status.")
                .Build()),
        });
}
