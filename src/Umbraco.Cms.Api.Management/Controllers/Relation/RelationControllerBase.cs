using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Relation;

[VersionedApiBackOfficeRoute("relation")]
[ApiExplorerSettings(GroupName = "Relation")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public abstract class RelationControllerBase : ManagementApiControllerBase
{
    protected IActionResult RelationOperationStatusResult(RelationOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            RelationOperationStatus.RelationTypeNotFound => BadRequest(problemDetailsBuilder
                .WithTitle("Relation type not found")
                .WithDetail("The relation type could not be found.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown relation operation status.")
                .Build()),
        });
}
