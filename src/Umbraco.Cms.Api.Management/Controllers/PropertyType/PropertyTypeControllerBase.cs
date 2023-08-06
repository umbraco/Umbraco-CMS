using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.PropertyType;

[ApiController]
[VersionedApiBackOfficeRoute("property-type")]
[ApiExplorerSettings(GroupName = "Property Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocumentTypes)]
public abstract class PropertyTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult PropertyTypeOperationStatusResult(PropertyTypeOperationStatus status) =>
        status switch
        {
            PropertyTypeOperationStatus.ContentTypeNotFound => NotFound("The content type was not found."),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown data type operation status")
        };
}
