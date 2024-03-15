using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.RelationType}")]
[ApiExplorerSettings(GroupName = "Relation Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessRelationTypes)]
public class RelationTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult RelationTypeOperationStatusResult(RelationTypeOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            RelationTypeOperationStatus.InvalidId => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid id")
                .WithDetail("Can not assign an Id when creating a relation type")
                .Build()),
            RelationTypeOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the relation type operation.")
                .Build()),
            RelationTypeOperationStatus.KeyAlreadyExists => BadRequest(problemDetailsBuilder
                .WithTitle("Key already exists")
                .WithDetail("An entity with the given key already exists")
                .Build()),
            RelationTypeOperationStatus.NotFound => RelationTypeNotFound(problemDetailsBuilder),
            RelationTypeOperationStatus.InvalidChildObjectType => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid child object type")
                .WithDetail("The child object type is not allowed")
                .Build()),
            RelationTypeOperationStatus.InvalidParentObjectType => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid parent object type")
                .WithDetail("The parent object type is not allowed")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown relation type operation status.")
                .Build()),
        });

    protected IActionResult RelationTypeNotFound() => OperationStatusResult(RelationTypeOperationStatus.NotFound, RelationTypeNotFound);

    private IActionResult RelationTypeNotFound(ProblemDetailsBuilder problemDetailsBuilder) => NotFound(problemDetailsBuilder
        .WithTitle("Relation type not found")
        .WithDetail("A relation type with the given key does not exist")
        .Build());
}
