using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
public class ScriptControllerBase : ManagementApiControllerBase
{
    protected IActionResult ScriptOperationStatusResult(ScriptOperationStatus status) =>
        status switch
        {
            ScriptOperationStatus.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown script operation status")
        };
}
