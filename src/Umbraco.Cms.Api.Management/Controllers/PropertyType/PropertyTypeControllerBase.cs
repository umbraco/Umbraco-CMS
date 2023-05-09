using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PropertyType;

[ApiController]
[VersionedApiBackOfficeRoute("property-type")]
[ApiExplorerSettings(GroupName = "Property Type")]
public abstract class PropertyTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult PropertyTypeOperationStatusResult(PropertyTypeOperationStatus status) =>
        status switch
        {
            PropertyTypeOperationStatus.ContentTypeNotFound => NotFound("The content type was not found."),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown data type operation status")
        };
}
