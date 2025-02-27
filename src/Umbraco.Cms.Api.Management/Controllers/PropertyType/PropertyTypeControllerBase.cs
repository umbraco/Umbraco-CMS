using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.PropertyType;

[VersionedApiBackOfficeRoute("property-type")]
[ApiExplorerSettings(GroupName = "Property Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public abstract class PropertyTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult PropertyTypeOperationStatusResult(PropertyTypeOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            PropertyTypeOperationStatus.ContentTypeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The content type was not found.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown property type operation status.")
                .Build()),
        });
}
