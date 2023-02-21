using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

[ApiController]
[VersionedApiBackOfficeRoute("relation-type")]
[ApiExplorerSettings(GroupName = "RelationType")]
[ApiVersion("1.0")]
public class RelationTypeControllerBase : ManagementApiControllerBase
{
        protected IActionResult RelationTypeOperationStatusResult(RelationTypeOperationStatus status) =>
        status switch
        {
            RelationTypeOperationStatus.InvalidId => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid id")
                .WithDetail("Can not assign an Id when creating a relation type")
                .Build()),
            RelationTypeOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the relation type operation.")
                .Build()),
            RelationTypeOperationStatus.KeyAlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Key already exists")
                .WithDetail("An entity with the given key already exists")
                .Build()),
            RelationTypeOperationStatus.NotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Relation type not found")
                .WithDetail("A relation type with the given key does not exist")
                .Build()),
        };
}
