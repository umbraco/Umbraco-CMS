using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = "Stylesheet")]
public class StylesheetBaseController : ManagementApiControllerBase
{
    protected IActionResult StylesheetOperationStatusResult(StylesheetOperationStatus status) =>
        status switch
        {
            StylesheetOperationStatus.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown script operation status"),
        };
}
