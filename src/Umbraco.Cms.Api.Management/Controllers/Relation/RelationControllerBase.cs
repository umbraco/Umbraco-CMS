using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Relation;

[ApiController]
[VersionedApiBackOfficeRoute("relation")]
[ApiExplorerSettings(GroupName = "Relation")]
[ApiVersion("1.0")]
// TODO: Implement Authentication
public abstract class RelationControllerBase : ManagementApiControllerBase
{
    protected IActionResult RelationOperationStatusResult(RelationOperationStatus status) =>
        status switch
        {
            RelationOperationStatus.NotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid fallback language")
                .WithDetail("The fallback language could not be applied. This may be caused if the fallback language causes cyclic fallbacks.")
                .Build()),
        };
}
