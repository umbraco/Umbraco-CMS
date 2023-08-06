using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Relation;

[ApiController]
[VersionedApiBackOfficeRoute("relation")]
[ApiExplorerSettings(GroupName = "Relation")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessContent)]
public abstract class RelationControllerBase : ManagementApiControllerBase
{
    protected IActionResult RelationOperationStatusResult(RelationOperationStatus status) =>
        status switch
        {
            RelationOperationStatus.RelationTypeNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Relation type not found")
                .WithDetail("The relation type could not be found.")
                .Build()),
        };
}
