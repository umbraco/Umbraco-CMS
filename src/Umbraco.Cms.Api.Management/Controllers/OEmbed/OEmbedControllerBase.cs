using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.OEmbed;

[VersionedApiBackOfficeRoute("oembed")]
[ApiExplorerSettings(GroupName = "oEmbed")]
public abstract class OEmbedControllerBase : ManagementApiControllerBase
{
     protected IActionResult OEmbedOperationStatusResult(OEmbedOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            OEmbedOperationStatus.NoSupportedProvider => BadRequest(problemDetailsBuilder
                .WithTitle("The specified url is not supported.")
                .WithDetail("No oEmbed provider was found for the specified url.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown oEmbed operation status.")
                .Build()),
        });

}
